using System;
using BinarySerialization;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using LMS5xxConnector.Telegram.CommandContainers;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector
{
    public class Lms5XxConnectorB : IDisposable
    {
        private CancellationTokenSource _tokenSource = new();
        private CancellationToken _token;
        private (TcpClient Value, bool IsInternal)? _tcpClient;
        private NetworkStream _networkStream = default!;
        private readonly BinarySerializer _serializer = new()
        {
          Endianness = Endianness.Big
        };
        private readonly ILogger<Lms5XxConnectorB> _logger;
        private readonly ASCIIEncoding _encoding = new();

        public int ConnectTimeout { get; set; } = DefaultConnectTimeout;
        public int ReadTimeout { get; set; } = Timeout.Infinite;
        public int WriteTimeout { get; set; } = Timeout.Infinite;
        public bool IsDebug { get; set; } = true;
        public bool IsConnected => _tcpClient?.Value.Connected ?? false;
        
        internal static int DefaultConnectTimeout { get; set; } = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
        
        public ConcurrentDictionary<(CommandTypes, Commands), Action<TelegramContentB>> ReceivedHandles = new();
        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        public Lms5XxConnectorB(ILogger<Lms5XxConnectorB>? logger = null)
        {
            _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<Lms5XxConnectorB>();
        }

        public async Task ConnectAsync()
        {
            _logger?.LogInformation("开始连接到本地回环地址，端口：2111");
            await ConnectAsync(new IPEndPoint(IPAddress.Loopback, 2111));
        }

        public async Task ConnectAsync(string remoteEndpoint)
        {
            _logger?.LogInformation("开始解析并连接到终端点：{Endpoint}", remoteEndpoint);
            
            if (!TcpUtils.TryParseEndpoint(remoteEndpoint.AsSpan(), out var parsedRemoteEndpoint))
            {
                _logger?.LogError("无效的终端点格式：{Endpoint}", remoteEndpoint);
                throw new FormatException("指定的终端点格式无效");
            }

            await ConnectAsync(parsedRemoteEndpoint);
        }

        public async Task ConnectAsync(IPEndPoint remoteEndpoint)
        {
            _logger?.LogInformation("开始连接到远程设备 {Address}:{Port}", remoteEndpoint.Address, remoteEndpoint.Port);

            try 
            {
                await Initialize(new TcpClient(), remoteEndpoint);
                _logger?.LogInformation("成功连接到远程设备");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "连接失败");
                throw;
            }
        }

        private async Task Initialize(TcpClient tcpClient, IPEndPoint? remoteEndpoint)
        {
            if (_tcpClient is { IsInternal: true })
            {
                _logger.LogDebug("关闭旧的TCP连接");
                _tcpClient.Value.Value.Close();
            }

            var isInternal = remoteEndpoint is not null;
            _tcpClient = (tcpClient, isInternal);

            if (remoteEndpoint is not null)
            {
                if (!tcpClient.ConnectAsync(remoteEndpoint.Address, remoteEndpoint.Port)
                        .Wait(ConnectTimeout))
                {
                    throw new TimeoutException("无法在指定时间内建立连接");
                }
            }

            _networkStream = tcpClient.GetStream();

            if (isInternal)
            {
                _networkStream.ReadTimeout = ReadTimeout;
                _networkStream.WriteTimeout = WriteTimeout;
            }

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            
            // 启动接收循环
            _ = Task.Run(ReceiveLoop, _token);
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (!_token.IsCancellationRequested)
                {
                    var packet = await _serializer.DeserializeAsync<CoLaB>(_networkStream, cancellationToken: _token);
                    
                    if (packet?.Content != null)
                    {
                        if (ReceivedHandles.TryGetValue((packet.Content.CommandTypes, packet.Content.Payload.Command), 
                            out var handler))
                        {
                            handler(packet.Content);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "接收数据时发生错误");
            }
        }

        public async Task Send(TelegramContentB data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!IsConnected) throw new IOException("未连接到服务器");

            var telegram = new CoLaB { Content = data };
            
            try
            {
                if (IsDebug)
                {
                    using var stream = new MemoryStream();
                    await _serializer.SerializeAsync(stream, telegram, cancellationToken: _token);
                    var buffer = stream.ToArray();
                    _logger.LogDebug("发送报文: {Telegram}", _encoding.GetString(buffer));
                    await _networkStream.WriteAsync(buffer, _token);
                }
                else
                {
                    await _serializer.SerializeAsync(_networkStream, telegram, cancellationToken: _token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送数据失败");
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                _tokenSource.Cancel();
                _networkStream?.Close();
                _tcpClient?.Value.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开连接时发生错误");
            }
        }

        public void Dispose()
        {
            Disconnect();
            _tokenSource.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<UniqueIdentificationModeCommandB?> GetDeviceInfo()
        {
            var telegram = new TelegramContentB
            {
                CommandTypes = CommandTypes.Srn,
                Payload = new SrnCommandContainerB
                {
                    Command = Commands.DeviceIdent
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<UniqueIdentificationModeCommandB?>();

            if (ReceivedHandles.TryAdd((CommandTypes.Sra, Commands.DeviceIdent),
                (telegramContent) =>
                {
                    ReceivedHandles.Remove( (CommandTypes.Sra, Commands.DeviceIdent) , out _);
                    var deviceInfo = (UniqueIdentificationModeCommandB)telegramContent.Payload.CommandConnent;
                    tcs.TrySetResult(deviceInfo);
                }))
            {
                try 
                {
                    await Send(telegram);
                    cancellationTokenSource.CancelAfter(3000); // 3秒超时
                    cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());

                    return await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("获取设备信息超时");
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "获取设备信息失败");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("还有上次未完成的设备信息查询命令");
                return null;
            }
        }
    }
} 