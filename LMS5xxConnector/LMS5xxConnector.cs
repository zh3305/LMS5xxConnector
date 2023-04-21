using BinarySerialization;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS5xxConnector.Telegram.CommandContainers;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector
{
    public class Lms5XxConnector : IDisposable
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


        /// <summary>
        /// Connect to localhost at port 2111. 
        /// </summary>
        public async Task ConnectAsync()
        {
            await ConnectAsync(new IPEndPoint(IPAddress.Loopback, 2111));
        }

        /// <summary>
        /// Connect to the specified <paramref name="remoteEndpoint"/>.
        /// </summary>
        /// <param name="remoteEndpoint">The IP address and optional port of the end unit. Examples: "192.168.0.1", "192.168.0.1:502", "::1", "[::1]:502". The default port is 502.</param>
        public async Task ConnectAsync(string remoteEndpoint)
        {
            if (!TcpUtils.TryParseEndpoint(remoteEndpoint.AsSpan(), out var parsedRemoteEndpoint))
                throw new FormatException("An invalid IPEndPoint was specified.");

#if NETSTANDARD2_0
         await   Connect(parsedRemoteEndpoint!);
#endif

#if NETSTANDARD2_1_OR_GREATER
            await ConnectAsync(parsedRemoteEndpoint);
#endif
        }


        /// <summary>
        /// Connect to the specified <paramref name="remoteIpAddress"/> at port 502.
        /// </summary>
        /// <param name="remoteIpAddress">The IP address of the end unit. Example: IPAddress.Parse("192.168.0.1").</param>
        public async Task ConnectAsync(IPAddress remoteIpAddress)
        {
            await ConnectAsync(new IPEndPoint(remoteIpAddress, 2111));
        }

        /// <summary>
        /// Event to call when byte data has become available from the server.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;


        public ConcurrentDictionary<(CommandTypes, Commands), Action<TelegramContent>> ReceivedHandles = new()
        {
        };


        /// <summary>
        /// Connect to the specified <paramref name="remoteEndpoint"/>.
        /// </summary>
        /// <param name="remoteEndpoint">The IP address and port of the end unit.</param>
        public async Task ConnectAsync(IPEndPoint remoteEndpoint)
        {
            await Initialize(new TcpClient(), remoteEndpoint);
        }

        private async Task Initialize(TcpClient tcpClient, IPEndPoint? remoteEndpoint)
        {
            if (_tcpClient.HasValue && _tcpClient.Value.IsInternal)
                _tcpClient.Value.Value.Close();

            var isInternal = remoteEndpoint is not null;
            _tcpClient = (tcpClient, isInternal);

            if (remoteEndpoint is not null && !tcpClient.ConnectAsync(remoteEndpoint.Address, remoteEndpoint.Port)
                    .Wait(ConnectTimeout))
                throw new TimeoutException("Could not connect within the specified time.");

            // Why no method signature with NetworkStream only and then set the timeouts 
            // in the Connect method like for the RTU client?
            //
            // "If a NetworkStream was associated with a TcpClient, the Close method will
            //  close the TCP connection, but not dispose of the associated TcpClient."
            // -> https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream.close?view=net-6.0

            _networkStream = tcpClient.GetStream();

            if (isInternal)
            {
                _networkStream.ReadTimeout = ReadTimeout;
                _networkStream.WriteTimeout = WriteTimeout;
            }


            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _token.Register(() => { _networkStream?.Close(); tcpClient.Close(); });
           _= Task.Run(() => DataReceiver(_token), _token);
            // // Wait for closed connection
            // await networkReadTask;
          
        }

        private async Task DataReceiver(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _tcpClient is { Value: { Connected: true } })
            {
                try
                {
                    var packetCoLaB = await _serializer.DeserializeAsync<PacketCoLaB>(_networkStream);
                    // Console.WriteLine(JsonConvert.SerializeObject(packetCoLaB,
                    //     new JsonSerializerSettings
                    //     {
                    //         Converters = new List<JsonConverter> { new StringEnumConverter() }
                    //     }));
                    if (packetCoLaB.Content == null)
                    {
                        Console.WriteLine("-----------  数据包解析错误------------------------");
                        continue;
                    }

                    //处理包数据
                    // HandlerTelegram(packetCoLaB);
                    if (ReceivedHandles.TryGetValue(
                            (packetCoLaB.Content.CommandTypes, packetCoLaB.Content.Payload.Command), out var handle))
                    {
                        handle(packetCoLaB.Content);
                    }
                }
                finally
                {
                }
                // catch (AggregateException)
                // {
                //     Logger?.Invoke($"{_header}data receiver canceled, disconnected");
                //     break;
                // }
                // catch (IOException)
                // {
                //     Logger?.Invoke($"{_header}data receiver canceled, disconnected");
                //     break;
                // }
                // catch (SocketException)
                // {
                //     Logger?.Invoke($"{_header}data receiver canceled, disconnected");
                //     break;
                // }
                // catch (TaskCanceledException)
                // {
                //     Logger?.Invoke($"{_header}data receiver task canceled, disconnected");
                //     break;
                // }
                // catch (OperationCanceledException)
                // {
                //     Logger?.Invoke($"{_header}data receiver operation canceled, disconnected");
                //     break;
                // }
                // catch (ObjectDisposedException)
                // {
                //     Logger?.Invoke($"{_header}data receiver canceled due to disposal, disconnected");
                //     break;
                // }
                // catch (Exception e)
                // {
                //     Logger?.Invoke($"{_header}data receiver exception:{Environment.NewLine}{e}{Environment.NewLine}");
                //     break;
                // }
            }
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

        // async Task TransceiveFrameAsync(PacketCoLaB packetCoLaB, CancellationToken cancellationToken = default)
        // {
        //     await _serializer.SerializeAsync(_networkStream, packetCoLaB, cancellationToken: cancellationToken).ConfigureAwait(false);
        // }

        public async Task SendTelegram(PacketCoLaB packetCoLaB, CancellationToken cancellationToken = default)
        {
            await _serializer.SerializeAsync(_networkStream, packetCoLaB, cancellationToken: cancellationToken)
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
                        ReceivedHandles.Remove((CommandTypes.Sra, Commands.LMDscandata),out _); 
                        tcs.TrySetResult((LMDscandataModeCommand)telegramContent.Payload.CommandConnent);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());
                
                return  await tcs.Task;
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
                        ReceivedHandles.Remove((CommandTypes.Sea, Commands.LMDscandata),out _); 
                        
                        tcs.TrySetResult(((ConfirmationRequestModeCommandBase)telegramContent.Payload.CommandConnent).StopStart);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());
                
                return  await tcs.Task;
            }
            else
            {
                //还有上次未完成的相同命令
                Console.WriteLine("还有上次未完成的相同命令");
                //抛出重复执行异常
                throw  new Exception("还有上次未完成的相同命令");
                
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
                        ReceivedHandles.Remove((CommandTypes.Sea, Commands.LMDscandata),out _); 
                        
                        tcs.TrySetResult(((ConfirmationRequestModeCommandBase)telegramContent.Payload.CommandConnent).StopStart);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());
                
                return  await tcs.Task;
            }
            else
            {
                //还有上次未完成的相同命令
                Console.WriteLine("还有上次未完成的相同命令");
                //抛出重复执行异常
                throw  new Exception("还有上次未完成的相同命令");
                
            }

        }
        public async Task<UniqueIdentificationModeCommand?> GetDeviceInfo()
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
                        ReceivedHandles.Remove((CommandTypes.Sra, Commands.DeviceIdent),out _); 
                        tcs.TrySetResult((UniqueIdentificationModeCommand)telegramContent.Payload.CommandConnent);
                    }))
            {
                await Send(telegram);
                cancellationTokenSource.CancelAfter(3000);//3秒后取消
                cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());
                
                return  await tcs.Task;
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
            var telegram = new PacketCoLaB
            {
                Content = data
            };
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

        //是否调试输出
        public bool IsDebug { get; set; } = true;
        readonly ASCIIEncoding _encoding = new ASCIIEncoding();

    }
}