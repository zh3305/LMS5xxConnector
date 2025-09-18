using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

/// <summary>
/// 雷达数据播放器
/// </summary>
public class RadarDataPlayer : IDisposable
{
    private readonly string _dataFilePath;
    private readonly ILogger _logger;
    private readonly object _fileLock = new object();
    private FileStream _fileStream;
    private BinaryReader _reader;
    private bool _isDisposed;

    public RadarDataPlayer(string dataFilePath, ILogger logger)
    {
        _dataFilePath = dataFilePath;
        _logger = logger;
        OpenFile();
    }

    private void OpenFile()
    {
        _fileStream = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _reader = new BinaryReader(_fileStream);
    }

    public RadarRawData ReadNextFrame()
    {
        try
        {
            if (_fileStream.Position >= _fileStream.Length)
            {
                throw new EndOfStreamException("已到达文件末尾");
            }

            var frame = new RadarRawData
            {
                Timestamp = DateTime.FromBinary(_reader.ReadInt64()),
                RadarName = _reader.ReadString()
            };

            var dataLength = _reader.ReadInt32();
            frame.RawBytes = _reader.ReadBytes(dataLength);

            return frame;
        }
        catch (EndOfStreamException)
        {
            throw;
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _reader?.Dispose();
            _fileStream?.Dispose();
            _isDisposed = true;
        }
    }
}

class Program
{
    private static TcpListener _tcpServer;
    private static bool _isServerRunning;
    private static bool _isScanning = false;
    private static CancellationTokenSource _scanCts = new CancellationTokenSource();
    private static ILogger _logger;
    // private static string _debugDataPath = "202503052461fd86-1b2e-4266-b3a0-ba8f288d80ef_Unknown.dat"; // 雷达数据文件路径
    // private static string _debugDataPath = "20250305463f25e8-7e1c-4744-8982-87dc20e699f6_Unknown.dat"; // 雷达数据文件路径
    // private static string _debugDataPath = "2025030534fcbfca-081d-490f-9912-8d31fd77bb80_Unknown.dat"; // 雷达数据文件路径
    // private static string _debugDataPath = "20250213d416802a-9d18-4a5d-a53e-f635787028dd_Unknown.dat"; // 雷达数据文件路径
    private static string _debugDataPath = "202507249df4ce25-97d4-4b97-a61f-5fcfd56c46bd_Unknown.dat"; // 标定数据
    private static List<byte[]> _lmdDataFrames = new List<byte[]>();

    static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole()
                  .SetMinimumLevel(LogLevel.Debug);
        });
        _logger = loggerFactory.CreateLogger<Program>();

        // 预加载LMDscandata数据
        LoadLMDscandata();

        _tcpServer = new TcpListener(IPAddress.Any, 2111);
        _tcpServer.Start();
        _isServerRunning = true;
        _logger.LogInformation("LMS5xx模拟服务器已启动,监听端口:2111");

        while (_isServerRunning)
        {
            try
            {
                var client = await _tcpServer.AcceptTcpClientAsync();
                _logger.LogInformation("客户端已连接");
                HandleClientCommunication(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理客户端连接时发生错误");
            }
        }
    }

    private static void HandleClientCommunication(TcpClient client)
    {
        Task.Run(async () =>
        {
            try
            {
                using var stream = client.GetStream();
                var buffer = new byte[1024];

                while (client.Connected && _isServerRunning)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = await stream.ReadAsync(buffer);
                        if (bytesRead > 0)
                        {
                            string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            _logger.LogDebug($"收到数据: {received}");

                            if (received.Contains("sRN DeviceIdent"))
                            {
                                string response = "\x02sRA DeviceIdent 10 LMS10x_FieldEval 10 V1.36-21.10.2010\x03";
                                byte[] responseData = Encoding.ASCII.GetBytes(response);
                                await stream.WriteAsync(responseData);
                                _logger.LogDebug($"发送设备信息响应");
                            }
                            else if (received.Contains("sEN LMDscandata"))
                            {
                                if (received.Contains("1"))
                                {
                                    _isScanning = true;
                                    _scanCts = new CancellationTokenSource();
                                    string response = "\x02sEA LMDscandata 1\x03";
                                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                                    await stream.WriteAsync(responseData);
                                    _logger.LogInformation("开始扫描数据模拟");
                                    StartScanDataSimulation(stream, _scanCts.Token);
                                }
                                else if (received.Contains("0"))
                                {
                                    _isScanning = false;
                                    _scanCts.Cancel();
                                    string response = "\x02sEA LMDscandata 0\x03";
                                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                                    await stream.WriteAsync(responseData);
                                    _logger.LogInformation("停止扫描数据模拟");
                                }
                            }
                        }
                    }
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理客户端通信时发生错误");
            }
            finally
            {
                client.Close();
                _logger.LogInformation("客户端连接已关闭");
            }
        });
    }

    private static void LoadLMDscandata()
    {
        try
        {
            using var dataPlayer = new RadarDataPlayer(_debugDataPath, _logger);
            try
            {
                while (true)
                {
                    var frame = dataPlayer.ReadNextFrame();
                    // 检查数据是否包含LMDscandata
                    if (frame.RawBytes != null && 
                        Encoding.ASCII.GetString(frame.RawBytes).Contains("LMDscandata"))
                    {
                        _lmdDataFrames.Add(frame.RawBytes);
                        _logger.LogDebug($"已加载LMDscandata帧，当前共 {_lmdDataFrames.Count} 帧");
                    }
                }
            }
            catch (EndOfStreamException)
            {
                _logger.LogInformation($"数据文件读取完成，共加载 {_lmdDataFrames.Count} 帧LMDscandata数据");
            }

            if (_lmdDataFrames.Count == 0)
            {
                throw new InvalidOperationException("未找到任何LMDscandata数据");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载LMDscandata数据时发生错误");
            throw;
        }
    }

    private static void StartScanDataSimulation(NetworkStream stream, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                if (_lmdDataFrames.Count == 0)
                {
                    _logger.LogError("没有可用的LMDscandata数据");
                    return;
                }

                var frameCounter = 0;
                var currentFrameIndex = 0;
                var targetFrequency = 100; // 目标频率：100Hz
                var targetTicksPerFrame = TimeSpan.TicksPerSecond / targetFrequency;
                var logInterval = TimeSpan.FromSeconds(5); // 每5秒记录一次统计信息
                var lastLogTime = DateTime.UtcNow;
                var framesSinceLastLog = 0;
                var nextFrameTime = DateTime.UtcNow.Ticks;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var currentTicks = DateTime.UtcNow.Ticks;
                    
                    if (currentTicks >= nextFrameTime)
                    {
                        // 发送当前帧
                        await stream.WriteAsync(_lmdDataFrames[currentFrameIndex], cancellationToken);
                        
                        // 更新计数器和索引
                        currentFrameIndex = (currentFrameIndex + 1) % _lmdDataFrames.Count;
                        frameCounter++;
                        framesSinceLastLog++;

                        // 计算下一帧的目标时间
                        nextFrameTime += targetTicksPerFrame;
                        
                        // 如果已经落后太多，重新调整基准时间
                        if (currentTicks > nextFrameTime + targetTicksPerFrame)
                        {
                            nextFrameTime = currentTicks + targetTicksPerFrame;
                            _logger.LogWarning("帧发送延迟，重新调整定时");
                        }

                        // 每5秒记录一次统计信息
                        var now = DateTime.UtcNow;
                        if (now - lastLogTime >= logInterval)
                        {
                            var actualHz = framesSinceLastLog / (now - lastLogTime).TotalSeconds;
                            _logger.LogInformation($"当前发送速率: {actualHz:F1} Hz (已发送 {frameCounter} 帧)");
                            framesSinceLastLog = 0;
                            lastLogTime = now;
                        }
                    }
                    else
                    {
                        // 使用自旋等待来提高精度
                        if (nextFrameTime - currentTicks > TimeSpan.TicksPerMillisecond * 2)
                        {
                            await Task.Delay(1, cancellationToken);
                        }
                        else
                        {
                            Thread.SpinWait(10);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("扫描数据模拟已取消");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "扫描数据模拟时发生错误");
            }
        }, cancellationToken);
    }
}