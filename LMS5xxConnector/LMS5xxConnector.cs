using BinarySerialization;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LMS5xxConnector.Telegram.CommandContainers;
using LMS5xxConnector.Telegram.CommandContents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LMS5xxConnector
{
    /// <summary>
    /// LMS5xx 雷达设备连接器实现，使用CoLaA协议与雷达设备通信
    /// </summary>
    public class Lms5XxConnector : ILms5XxConnector
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token;

        private (TcpClient Value, bool IsInternal)? _tcpClient;
        private NetworkStream _networkStream = default!;

        BinarySerializer _serializer = new BinarySerializer();



        /// <summary>
        /// Gets or sets the connect timeout in milliseconds. Default is 1000 ms.
        /// </summary>
        public int ConnectTimeout { get; set; } = DefaultConnectTimeout;

        /// <summary>
        /// Gets or sets the read timeout in milliseconds. Default is <see cref="Timeout.Infinite"/>.
        /// </summary>
        public int ReadTimeout { get; set; } = Timeout.Infinite;

        /// <summary>
        /// Gets or sets the write timeout in milliseconds. Default is <see cref="Timeout.Infinite"/>.
        /// </summary>
        public int WriteTimeout { get; set; } = Timeout.Infinite;

        internal static int DefaultConnectTimeout { get; set; } = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;


        /// <summary>
        /// Gets the connection status of the underlying TCP client.
        /// </summary>
        public bool IsConnected => _tcpClient?.Value.Connected ?? false;
        private readonly ILogger<Lms5XxConnector> _logger;

        private string _radarName;
        private readonly string _debugDataPath;
        private readonly object _fileLock = new object();


        /// <summary>
        /// 连接到本地回环地址的2111端口
        /// </summary>
        public async Task ConnectAsync()
        {
            _logger?.LogInformation("开始连接到本地回环地址，端口：2111");
            await ConnectAsync(new IPEndPoint(IPAddress.Loopback, 2111));
        }

        /// <summary>
        /// 连接到指定的远程终端点（字符串格式）
        /// </summary>
        /// <param name="remoteEndpoint">远程终端点字符串，格式：IP:端口，例如："192.168.0.1:2111"</param>
        public async Task ConnectAsync(string remoteEndpoint)
        {
            _logger?.LogInformation("开始解析并连接到终端点：{Endpoint}", remoteEndpoint);

            if (!TcpUtils.TryParseEndpoint(remoteEndpoint.AsSpan(), out var parsedRemoteEndpoint))
            {
                _logger?.LogError("无效的终端点格式：{Endpoint}", remoteEndpoint);
                throw new FormatException("指定的终端点格式无效");
            }

#if NETSTANDARD2_0
         await   Connect(parsedRemoteEndpoint!);
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            await ConnectAsync(parsedRemoteEndpoint);
#endif
        }


        /// <summary>
        /// 连接到指定IP地址的2111端口
        /// </summary>
        /// <param name="remoteIpAddress">远程设备的IP地址</param>
        public async Task ConnectAsync(IPAddress remoteIpAddress)
        {
            _logger?.LogInformation("开始连接到IP地址：{Address}，端口：2111", remoteIpAddress);
            await ConnectAsync(new IPEndPoint(remoteIpAddress, 2111));
        }

        /// <summary>
        /// Event to call when byte data has become available from the server.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;


        public ConcurrentDictionary<(CommandTypes, Commands), Action<TelegramContent>> ReceivedHandles = new() { };


        /// <summary>
        /// 连接到指定的远程终端点
        /// </summary>
        /// <param name="remoteEndpoint">远程设备的IP地址和端口</param>
        public async Task ConnectAsync(IPEndPoint remoteEndpoint)
        {
            _logger?.LogInformation("开始连接到远程设备 {Address}:{Port}", remoteEndpoint.Address, remoteEndpoint.Port);

            try
            {
                await Initialize(new TcpClient(), remoteEndpoint);
                _logger?.LogInformation("成功连接到远程设备");
            }
            catch (TimeoutException)
            {
                _logger?.LogError("连接超时，请检查网络连接和设备状态");
                throw;
            }
            catch (SocketException ex)
            {
                _logger?.LogError(ex, "网络连接失败：{Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "连接过程中发生未知错误");
                throw;
            }
        }

        /// <summary>
        /// 初始化TCP连接和数据接收
        /// </summary>
        /// <param name="tcpClient">TCP客户端实例</param>
        /// <param name="remoteEndpoint">远程终端点</param>
        private async Task Initialize(TcpClient tcpClient, IPEndPoint? remoteEndpoint)
        {
            // 如果存在旧的内部TCP客户端连接，先关闭它
            if (_tcpClient is { IsInternal: true })
            {
                _logger.LogDebug("关闭旧的TCP连接");
                _tcpClient.Value.Value.Close();
            }

            var isInternal = remoteEndpoint is not null;
            _tcpClient = (tcpClient, isInternal);

            if (remoteEndpoint is not null)
            {
                _logger?.LogDebug("开始建立TCP连接，超时时间：{Timeout}ms", ConnectTimeout);

                // 尝试在指定超时时间内建立连接
                if (!tcpClient.ConnectAsync(remoteEndpoint.Address, remoteEndpoint.Port)
                        .Wait(ConnectTimeout))
                {
                    _logger?.LogError("TCP连接超时");
                    throw new TimeoutException("无法在指定时间内建立连接");
                }
            }

            // 获取网络流
            _networkStream = tcpClient.GetStream();

            // 设置内部连接的超时参数
            if (isInternal)
            {
                _logger?.LogDebug("设置网络流超时参数 - 读取超时：{ReadTimeout}ms，写入超时：{WriteTimeout}ms", ReadTimeout, WriteTimeout);

                _networkStream.ReadTimeout = ReadTimeout;
                _networkStream.WriteTimeout = WriteTimeout;
            }

            // 初始化取消令牌
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            // 注册连接关闭回调
            _token.Register(() =>
            {
                _logger?.LogDebug("正在关闭网络连接");
                _networkStream?.Close();
                tcpClient.Close();
            });

            // 启动数据接收任务
            _logger?.LogDebug("启动数据接收任务");
            _ = Task.Run(() => DataReceiver(_token), _token);
        }

        private async Task DataReceiver(CancellationToken token)
        {
            Directory.CreateDirectory("DebugData");
            var buffer = new byte[8192]; // 8KB 缓冲区

            while (!token.IsCancellationRequested && _tcpClient is { Value: { Connected: true } })
            {
                CoLaA? packetCoLaB;
                try
                {
                    if (IsDebug)
                    {
                        // 使用内存流先捕获原始数据
                        using var memoryStream = new MemoryStream();
                        int bytesRead;

                        // 读取STX标记(0x02)
                        while ((bytesRead = await _networkStream.ReadAsync(buffer, 0, 1, token)) > 0)
                        {
                            if (buffer[0] == 0x02)
                            {
                                memoryStream.WriteByte(buffer[0]);
                                break;
                            }
                        }

                        // 读取直到ETX标记(0x03)
                        while ((bytesRead = await _networkStream.ReadAsync(buffer, 0, 1, token)) > 0)
                        {
                            memoryStream.WriteByte(buffer[0]);
                            if (buffer[0] == 0x03)
                            {
                                break;
                            }
                        }

                        // 记录原始数据
                        var rawData = new RadarRawData
                        {
                            Timestamp = DateTime.Now,
                            RadarName = _radarName,
                            RawBytes = memoryStream.ToArray()
                        };
                        // 保存原始数据
                        lock (_fileLock)
                        {
                            using var writer = new BinaryWriter(File.Open(_debugDataPath, FileMode.Append,
                                FileAccess.Write));
                            // 写入时间戳
                            writer.Write(rawData.Timestamp.ToBinary());
                            // 写入雷达名称
                            writer.Write(_radarName);
                            // 写入数据长度
                            writer.Write(rawData.RawBytes.Length);
                            // 写入原始数据
                            writer.Write(rawData.RawBytes);
                        }
                        // 将流位置重置到开始
                        memoryStream.Position = 0;

                        // 反序列化数据
                        packetCoLaB = await _serializer.DeserializeAsync<CoLaA>(memoryStream);

                    }
                    else
                    {
                        packetCoLaB = await _serializer.DeserializeAsync<CoLaA>(_networkStream);
                    }

                    if (packetCoLaB.Content == null)
                    {
                        _logger?.LogWarning("数据包解析错误");
                        continue;
                    }

                    var commandKey = (packetCoLaB.Content.CommandTypes, packetCoLaB.Content.Payload.Command);
                    if (ReceivedHandles.TryGetValue(commandKey, out var handle))
                    {
                        if (IsLongRunningHandler(commandKey))
                        {
                            _ = Task.Run(() => handle(packetCoLaB.Content), token)
                                .ContinueWith(t =>
                                {
                                    if (t.IsFaulted)
                                    {
                                        _logger?.LogError(t.Exception, "处理器执行失败");
                                    }
                                }, TaskContinuationOptions.OnlyOnFaulted);
                        }
                        else
                        {
                            handle(packetCoLaB.Content);
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger?.LogError(ex, "数据接收错误");
                    if (ex is IOException || ex is SocketException)
                    {
                        break;
                    }
                }
            }
        }

        private bool IsLongRunningHandler((CommandTypes, Commands) commandKey)
        {
            return commandKey switch
            {
                (CommandTypes.Sra, Commands.LMDscandata) => true,
                (CommandTypes.Sea, Commands.LMDscandata) => true,
                _ => false
            };
        }

        /// <summary>
        /// Disconnect from the end unit.
        /// </summary>
        public void Disconnect()
        {
            if (_tcpClient.HasValue && _tcpClient.Value.IsInternal)
                _tcpClient.Value.Value.Close();

            // _frameBuffer?.Dispose();

            // workaround for https://github.com/Apollo3zehn/FluentModbus/issues/44#issuecomment-747321152
            if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework",
                    StringComparison.OrdinalIgnoreCase))
                _tcpClient = null;
        }

        // async Task TransceiveFrameAsync(PacketCoLaA packetCoLaB, CancellationToken cancellationToken = default)
        // {
        //     await _serializer.SerializeAsync(_networkStream, packetCoLaB, cancellationToken: cancellationToken).ConfigureAwait(false);
        // }

        public async Task SendTelegram(CoLaA packetCoLaA, CancellationToken cancellationToken = default)
        {
            await _serializer.SerializeAsync(_networkStream, packetCoLaA, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }


        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task<LMDscandataModeCommand?> GetScanData()
        {
            var telegram = new TelegramContent
            {
                CommandTypes = CommandTypes.Srn,
                Payload = new SrnCommandContainer
                {
                    Command = Commands.LMDscandata
                }
            };
            var cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<LMDscandataModeCommand>();
            if (ReceivedHandles.TryAdd((CommandTypes.Sra, Commands.LMDscandata),
                    (telegramContent) =>
                    {
                        ReceivedHandles.Remove((CommandTypes.Sra, Commands.LMDscandata), out _);
                        tcs.TrySetResult((LMDscandataModeCommand)telegramContent.Payload.CommandConnent);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());

                return await tcs.Task;
            }
            else
            {
                //还有上次未完成的相同命令
                Console.WriteLine("还有上次未完成的相同命令");
            }

            return null;
        }
        public async Task<StopStart> StopScanData()
        {
            var telegram = new TelegramContent
            {
                CommandTypes = CommandTypes.Sen,
                Payload = new SenCommandContainer()
                {
                    Command = Commands.LMDscandata,
                    CommandConnent = new ConfirmationRequestModeCommandBase
                    {
                        StopStart = StopStart.Stop
                    }
                }
            };
            var cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<StopStart>();
            if (ReceivedHandles.TryAdd((CommandTypes.Sea, Commands.LMDscandata),
                    (telegramContent) =>
                    {
                        ReceivedHandles.Remove((CommandTypes.Sea, Commands.LMDscandata), out _);

                        tcs.TrySetResult(((ConfirmationRequestModeCommandBase)telegramContent.Payload.CommandConnent).StopStart);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());

                return await tcs.Task;
            }
            else
            {
                //还有上次未完成的相同命令
                Console.WriteLine("还有上次未完成的相同命令");
                //抛出重复执行异常
                throw new Exception("还有上次未完成的相同命令");

            }

        }
        public async Task<StopStart> StartScanData()
        {
            var telegram = new TelegramContent
            {
                CommandTypes = CommandTypes.Sen,
                Payload = new SenCommandContainer()
                {
                    Command = Commands.LMDscandata,
                    CommandConnent = new ConfirmationRequestModeCommandBase
                    {
                        StopStart = StopStart.Start
                    }
                }
            };
            var cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<StopStart>();
            if (ReceivedHandles.TryAdd((CommandTypes.Sea, Commands.LMDscandata),
                    (telegramContent) =>
                    {
                        ReceivedHandles.Remove((CommandTypes.Sea, Commands.LMDscandata), out _);

                        tcs.TrySetResult(((ConfirmationRequestModeCommandBase)telegramContent.Payload.CommandConnent).StopStart);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());

                return await tcs.Task;
            }
            else
            {
                //还有上次未完成的相同命令
                Console.WriteLine("还有上次未完成的相同命令");
                //抛出重复执行异常
                throw new Exception("还有上次未完成的相同命令");

            }

        }
        public async Task<UniqueIdentificationModeCommand?> GetDeviceInfo()
        {
            return await GetDeviceInfoInternal();
        }

        /// <summary>
        /// 获取设备信息的内部实现
        /// </summary>
        /// <returns>设备标识信息，如果获取失败则返回null</returns>
        private async Task<UniqueIdentificationModeCommand?> GetDeviceInfoInternal()
        {
            var telegram = new TelegramContent
            {
                CommandTypes = CommandTypes.Srn,
                Payload = new SrnCommandContainer
                {
                    Command = Commands.DeviceIdent
                }
            };
            var cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<UniqueIdentificationModeCommand>();
            if (ReceivedHandles.TryAdd((CommandTypes.Sra, Commands.DeviceIdent),
                    (telegramContent) =>
                    {
                        ReceivedHandles.Remove((CommandTypes.Sra, Commands.DeviceIdent), out _);
                        tcs.TrySetResult((UniqueIdentificationModeCommand)telegramContent.Payload.CommandConnent);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());

                return await tcs.Task;
            }
            else
            {
                //还有上次未完成的相同命令
                Console.WriteLine("还有上次未完成的相同命令");
            }

            return null;
        }


        /// <summary>
        /// Send data to the server.
        /// </summary>
        /// <param name="data">String containing data to send.</param>
        public async Task Send(TelegramContent data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!IsConnected) throw new IOException("Not connected to the server; use Connect() first.");
            var telegram = new CoLaA
            {
                Content = data
            };
            try
            {

                if (IsDebug)
                {
                    var stream = new MemoryStream();
                    await _serializer.SerializeAsync(stream, telegram, cancellationToken: _token);
                    var rbuffer = stream.ToArray();
                    //byte[] 转  ASCII 字符串
                    Console.WriteLine("SendTelegram:" + _encoding.GetString(rbuffer));
                    await _networkStream.WriteAsync(rbuffer, _token);
                }
                else
                {
                    await _serializer.SerializeAsync(_networkStream, telegram, cancellationToken: _token);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "序列化出错!");
                throw;
            }
        }

        //是否调试输出
        public bool IsDebug { get; set; } = false;
        readonly ASCIIEncoding _encoding = new ASCIIEncoding();

        public Lms5XxConnector(ILogger<Lms5XxConnector> logger, string radarName = "Unknown")
        {
            _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<Lms5XxConnector>();
            _radarName = radarName;
            _debugDataPath = Path.Combine("DebugData", $"{DateTime.Now:yyyyMMdd}{Guid.NewGuid()}_{radarName}.dat");
        }

        public async Task PlaybackDebugData(string debugDataFile, Action<DateTime, TelegramContent> callback)
        {
            using var reader = new BinaryReader(File.OpenRead(debugDataFile));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                try
                {
                    // 读取时间戳
                    var timestamp = DateTime.FromBinary(reader.ReadInt64());
                    // 读取雷达名称
                    var radarName = reader.ReadString();
                    // 读取数据长度
                    var dataLength = reader.ReadInt64();
                    // 读取原始数据
                    var rawData = reader.ReadBytes((int)dataLength);

                    // 反序列化数据
                    using var memoryStream = new MemoryStream(rawData);
                    var packetCoLaB = await _serializer.DeserializeAsync<CoLaA>(memoryStream);

                    if (packetCoLaB?.Content != null)
                    {
                        callback(timestamp, packetCoLaB.Content);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "回放调试数据时发生错误");
                }
            }
        }

    }
}