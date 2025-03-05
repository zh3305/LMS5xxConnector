using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LMS5xxConnector;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Telegram.CommandContents;
using CommunityToolkit.Mvvm.Input;
using InteractiveDataDisplay.WPF;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.IO;
using System.Text;
using System.Threading;

namespace DistanceSensorAppDemo;

// 添加自定义控制台格式化器类
[ObservableObject]
public partial class MainViewModel
{
    [ObservableProperty] private string _ipAddress;

    [ObservableProperty] private string _statusText;

    [ObservableProperty] private double _rotationAngle = 0;

    [ObservableProperty] private string _deviceInfo;

    private bool _isConnected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(InitializeCommand))]
    private bool _isInitialized;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isStarted;

    [ObservableProperty] private string _currentAction = "Ready";

    public CircleMarkerGraph CirclePoints;
    // [ObservableProperty]
    // public ObservableCollection<UIElement> charts = new ObservableCollection<UIElement>();

    private readonly Lms5XxConnector _connector;

    private readonly ILogger<MainViewModel> _logger;

    // 添加静态数组池
    private static readonly int MaxPointCount = 10000; // 根据实际最大点数调整

    private double[] _xsBuffer = new double[MaxPointCount];
    private double[] _ysBuffer = new double[MaxPointCount];
    private double[] _csBuffer = new double[MaxPointCount];

    [ObservableProperty] private bool _isSaveData;
    private FileStream? _saveFileStream;
    private string _currentSaveFilePath;
    private CancellationTokenSource? _playbackCancellationTokenSource;

    partial void OnIsSaveDataChanged(bool value)
    {
        if (_connector != null)
        {
            _connector.IsDebug = value;
        }
    }

    public MainViewModel()
    {
#if DEBUG
        IpAddress = "192.168.1.231";
#endif
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole(options =>
                {
                    options.FormatterName = "CustomFormatter";
                    // options.TimestampFormat = "HH:mm:ss.fff";
                })
                .AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>(options =>
                {
                    options.TimestampFormat = "HH:mm:ss.fff";
                });
        });
        _logger = loggerFactory.CreateLogger<MainViewModel>();
        _connector = new Lms5XxConnector(loggerFactory.CreateLogger<Lms5XxConnector>());
        _currentSaveFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LidarData",
            $"LidarData_{DateTime.Now:yyyyMMdd_HHmmss}.dat");
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsDisconnected));
            StartCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
            ConnectCommand.NotifyCanExecuteChanged();
            InitializeCommand.NotifyCanExecuteChanged();
            if (IsConnected)
            {
                StatusText = "Connected";
            }
            else
            {
                StatusText = "Disconnected";
            }
        }
    }

    public bool IsDisconnected => !IsConnected;

    [RelayCommand]
    public async Task ConnectAsync()
    {
        if (string.IsNullOrEmpty(_ipAddress))
        {
            MessageBox.Show("ip地址错误!");
            return;
        }

        try
        {
            await _connector.ConnectAsync(_ipAddress + ":2111");
        }
        catch (FormatException e)
        {
            MessageBox.Show("ip地址格式错误!" + e.Message);
            return;
        }
        catch (Exception e)
        {
            MessageBox.Show("设备链接失败!" + e.Message);
            return;
        }

        IsConnected = _connector.IsConnected;
        if (IsConnected)
        {
            await GetDeviceInfo();
        }
    }

    [RelayCommand]
    private async void Disconnect()
    {
        // TODO: Implement disconnect logic
        // Set IsConnected to false
        _connector.Disconnect();

        IsConnected = _connector.IsConnected;

        IsConnected = false;
        IsInitialized = false;
        IsStarted = false;

        if (_saveFileStream != null)
        {
            await _saveFileStream.DisposeAsync();
            _saveFileStream = null;
        }

        _playbackCancellationTokenSource?.Cancel();
    }

    private bool CanInitialize => IsConnected && !IsInitialized;

    [RelayCommand(CanExecute = nameof(CanInitialize))]
    private async Task Initialize()
    {
        IsInitialized = true;
    }

    private bool CanStart => IsConnected && !IsStarted;

    private bool CanStop => IsConnected; //&& IsStarted;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task Stop()
    {
        //开始连续采集
        CurrentAction = "set stop Send data permanently";
        if (await _connector.StopScanData() == StopStart.Stop)
        {
            CurrentAction = "set stop Send data permanently success!";
            IsStarted = false;
        }
        else
        {
            CurrentAction = "set  stop Send data permanently failed!";
        }
    }

    [RelayCommand]
    private async Task Restart()
    {
        // TODO: Implement restart logic
        // This should stop the acquisition, reinitialize the device, and start the acquisition again
    }

    [RelayCommand]
    private async Task Login()
    {
        // TODO: Implement login logic
        // This should prompt the user for credentials and perform a login
    }

    [RelayCommand]
    private async Task GetDeviceInfo()
    {
        // TODO: Implement get device info logic
        // This should retrieve device information and display it in DeviceInfo
        CurrentAction = "start GetDeviceInfo";
        var uniqueIdentificationModeCommand = await _connector.GetDeviceInfo();
        this.DeviceInfo = uniqueIdentificationModeCommand?.Name + "   " + uniqueIdentificationModeCommand?.Version;
        CurrentAction = "Ready";
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        //开始连续采集
        CurrentAction = "set Send data permanently";
        IsStarted = true;
        if (await _connector.StartScanData() == StopStart.Start)
        {
            _connector.ReceivedHandles.AddOrUpdate((CommandTypes.Ssn, Commands.LMDscandata), HanderScanData,
                (tuple, action) => HanderScanData);
        }
        else
        {
            CurrentAction = "set Send data permanently failed!";
        }
    }

    [RelayCommand]
    private async Task GetData()
    {
        CurrentAction = "start GetData";
        var dscandataModeCommand = await _connector.GetScanData();

        if (dscandataModeCommand != null && dscandataModeCommand.OutputChannelList.Count > 0)
        {
            ShowScanData(dscandataModeCommand);
        }

        CurrentAction = "Ready";
    }

    private readonly Stopwatch _fpsStopwatch = new Stopwatch();
    private int _frameCount = 0;
    private double _currentFps = 0;
    private readonly object _fpsLock = new object();

    private Stopwatch _stopwatch = new Stopwatch();

    private void HanderScanData(TelegramContent telegramContent)
    {
        try
        {
            if (telegramContent.Payload.CommandConnent is LMDscandataModeCommand
                {
                    OutputChannelList.Count: > 0
                } scanDataCommand)
            {
                // 如果启用了保存，则保存数据
                if (IsSaveData)
                {
                    SaveScanData(scanDataCommand);
                }

                // 现有的显示处理代码
                _ = Task.Run(() => ShowScanData(scanDataCommand))
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            _logger?.LogError(t.Exception, "扫描数据处理失败");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "处理扫描数据时发生错误");
        }
    }

    private void ShowScanData(LMDscandataModeCommand lmDscandataModeCommand)
    {
        var lmdScandata = lmDscandataModeCommand.OutputChannelList[0];
        UpdateFps();

        int pointCount = lmdScandata.DistDatas.Count;
        // 确保不超过缓冲区大小
        if (pointCount > MaxPointCount)
        {
            _logger?.LogWarning("点数超出缓冲区大小: {PointCount} > {MaxPointCount}", pointCount, MaxPointCount);
            pointCount = MaxPointCount;
        }

        var processingStopwatch = new Stopwatch();
        processingStopwatch.Start();

        if (false && System.Runtime.Intrinsics.X86.Sse2.IsSupported)
        {
            ProcessPointsSimd(lmdScandata, lmDscandataModeCommand, _xsBuffer, _ysBuffer, _csBuffer, pointCount);
            processingStopwatch.Stop();
            _logger?.LogDebug("处理扫描数据: 点数={Count}, 输出通道数={ChannelCount}, FPS={Fps:F2}, SIMD处理时间={ProcessingTime}Ticks",
                pointCount,
                lmDscandataModeCommand.OutputChannel8BitList.Count,
                _currentFps,
                processingStopwatch.ElapsedTicks);
        }
        else
        {
            ProcessPointsParallel(lmdScandata, lmDscandataModeCommand, _xsBuffer, _ysBuffer, _csBuffer, pointCount);
            processingStopwatch.Stop();
            _logger?.LogDebug("处理扫描数据: 点数={Count}, 输出通道数={ChannelCount}, FPS={Fps:F2}, 并行处理时间={ProcessingTime}ms",
                pointCount,
                lmDscandataModeCommand.OutputChannel8BitList.Count,
                _currentFps,
                processingStopwatch.ElapsedMilliseconds);
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                CirclePoints.PlotColor(
                    _xsBuffer.AsSpan(0, pointCount).ToArray(),
                    _ysBuffer.AsSpan(0, pointCount).ToArray(),
                    _csBuffer.AsSpan(0, pointCount).ToArray());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "更新图形显示时发生错误");
            }
        });
    }

    private void ProcessPointsParallel(OutputChannel lmdScandata, LMDscandataModeCommand scanDataCommand, 
        double[] xs, double[] ys, double[] cs, int pointCount)
    {
        var scaleFactor = lmdScandata.ScaleFactor.Value;
        var startAngle = lmdScandata.StartAngle.Value / 10000.0;
        var angularStep = lmdScandata.AngularStepSize.Value / 10000.0;
        var hasRssiData = scanDataCommand.OutputChannel8BitList.Count > 0;

        Parallel.For(0, pointCount, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, i =>
        {
            var dim = scaleFactor * lmdScandata.DistDatas[i].Value;
            // 先计算角度，然后加上旋转角度，最后转换为弧度
            var angle = startAngle + i * angularStep + RotationAngle;
            var angleRad = angle * Math.PI / 180.0;

            xs[i] = dim * Math.Cos(angleRad);
            ys[i] = dim * Math.Sin(angleRad);
            cs[i] = hasRssiData ? scanDataCommand.OutputChannel8BitList[0].DistDatas[i].Value : 255;
        });
    }

    private void ProcessPointsSimd(OutputChannel lmdScandata, LMDscandataModeCommand scanDataCommand, 
        double[] xs, double[] ys, double[] cs, int pointCount)
    {
        var scaleFactor = lmdScandata.ScaleFactor.Value;
        var startAngle = lmdScandata.StartAngle.Value / 10000.0;
        var angularStep = lmdScandata.AngularStepSize.Value / 10000.0;

        // 使用SIMD优化的并行处理
        Parallel.For(0, pointCount / 2, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, i =>
        {
            var index = i * 2;
            ProcessTwoPoints(index, lmdScandata, scanDataCommand, scaleFactor, startAngle, angularStep, xs, ys, cs);
        });

        // 处理剩余的单个点
        if (pointCount % 2 != 0)
        {
            ProcessSinglePoint(pointCount - 1, lmdScandata, scanDataCommand, scaleFactor, startAngle, angularStep, xs, ys, cs);
        }
    }

    private void ProcessTwoPoints(int index, OutputChannel lmdScandata, LMDscandataModeCommand scanDataCommand, 
        double scaleFactor, double startAngle, double angularStep, 
        double[] xs, double[] ys, double[] cs)
    {
        var hasRssiData = scanDataCommand.OutputChannel8BitList.Count > 0;

        for (int offset = 0; offset < 2 && (index + offset) < lmdScandata.DistDatas.Count; offset++)
        {
            var currentIndex = index + offset;
            var dim = scaleFactor * lmdScandata.DistDatas[currentIndex].Value;
            var angle = startAngle + currentIndex * angularStep + RotationAngle;
            var angleRad = angle * Math.PI / 180.0;

            xs[currentIndex] = dim * Math.Cos(angleRad);
            ys[currentIndex] = dim * Math.Sin(angleRad);
            cs[currentIndex] = hasRssiData ? scanDataCommand.OutputChannel8BitList[0].DistDatas[currentIndex].Value : 255;
        }
    }

    private void ProcessSinglePoint(int index, OutputChannel lmdScandata, LMDscandataModeCommand scanDataCommand, 
        double scaleFactor, double startAngle, double angularStep, 
        double[] xs, double[] ys, double[] cs)
    {
        var dim = scaleFactor * lmdScandata.DistDatas[index].Value;
        var angle = startAngle + index * angularStep + RotationAngle;
        var angleRad = angle * Math.PI / 180.0;

        xs[index] = dim * Math.Cos(angleRad);
        ys[index] = dim * Math.Sin(angleRad);
        cs[index] = scanDataCommand.OutputChannel8BitList.Count > 0 
            ? scanDataCommand.OutputChannel8BitList[0].DistDatas[index].Value 
            : 255;
    }

    /// <summary>
    /// 更新FPS计算
    /// </summary>
    private void UpdateFps()
    {
        lock (_fpsLock)
        {
            if (!_fpsStopwatch.IsRunning)
            {
                _fpsStopwatch.Start();
                _frameCount = 0;
            }

            _frameCount++;

            // 每秒更新一次FPS
            if (_fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                _currentFps = _frameCount * 1000.0 / _fpsStopwatch.ElapsedMilliseconds;
                _frameCount = 0;
                _fpsStopwatch.Restart();
            }
        }
    }

    // 添加属性用于外部访问FPS
    public double CurrentFps => _currentFps;

    private double GetRssiColor(ushort value)
    {
        return value;
    }

    private void SaveScanData(LMDscandataModeCommand scanDataCommand)
    {
        try
        {
            if (_saveFileStream == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_currentSaveFilePath)!);
                _saveFileStream = new FileStream(_currentSaveFilePath, FileMode.Create, FileAccess.Write);
            }

            // 序列化数据
            using var writer = new BinaryWriter(_saveFileStream, Encoding.UTF8, true);
            // 写入时间
            writer.Write(DateTime.Now.Ticks);
            // 写入数据点数量
            writer.Write(scanDataCommand.OutputChannelList[0].DistDatas.Count);

            // 写入距离数据
            foreach (var data in scanDataCommand.OutputChannelList[0].DistDatas)
            {
                writer.Write(data.Value);
            }

            // 写入强度数据（如果有）
            writer.Write(scanDataCommand.OutputChannel8BitList.Count > 0);
            if (scanDataCommand.OutputChannel8BitList.Count > 0)
            {
                foreach (var data in scanDataCommand.OutputChannel8BitList[0].DistDatas)
                {
                    writer.Write(data.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "保存扫描数据时发生错误");
        }
    }

    [RelayCommand]
    private async Task OpenDataFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".dat",
            Filter = "Lidar Data Files (*.dat)|*.dat"
        };

        if (dialog.ShowDialog() == true)
        {
            _currentSaveFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task StartPlayback(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_currentSaveFilePath))
        {
            MessageBox.Show("请先选择数据文件");
            return;
        }
#if NET8_0
        await _playbackCancellationTokenSource?.CancelAsync()!;
#else
        
         _playbackCancellationTokenSource?.Cancel();

#endif
        _playbackCancellationTokenSource = new CancellationTokenSource();

        try
        {
            using var fileStream = new FileStream(_currentSaveFilePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            while (fileStream.Position < fileStream.Length && !_playbackCancellationTokenSource.Token.IsCancellationRequested)
            {
                // 读取时间戳
                var timestamp = reader.ReadInt64();
                // 读取点数
                var pointCount = reader.ReadInt32();

                var scanDataCommand = new LMDscandataModeCommand();
                var outputChannel = new OutputChannel();

                // 读取距离数据
                for (int i = 0; i < pointCount; i++)
                {
                    outputChannel.DistDatas.Add(new SickValue<ushort>()
                    {
                        Value = reader.ReadUInt16()
                    });
                }

                scanDataCommand.OutputChannelList.Add(outputChannel);

                // 读取是否有强度数据
                var hasRssiData = reader.ReadBoolean();
                if (hasRssiData)
                {
                    var rssiChannel = new OutputChannel();
                    for (int i = 0; i < pointCount; i++)
                    {
                        rssiChannel.DistDatas.Add(new SickValue<ushort>()
                        {
                            Value = reader.ReadByte()
                        });
                    }
                    scanDataCommand.OutputChannel8BitList.Add(rssiChannel);
                }

                await Task.Run(() => ShowScanData(scanDataCommand), cancellationToken);
                await Task.Delay(50, cancellationToken); // 控制回放速度
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "回放数据时发生错误");
            MessageBox.Show($"回放出错: {ex.Message}");
        }
    }

    [RelayCommand]
    private void StopPlayback()
    {
        _playbackCancellationTokenSource?.Cancel();
    }
}