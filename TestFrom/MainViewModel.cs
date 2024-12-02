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

namespace DistanceSensorAppDemo;

[ObservableObject]
public partial class MainViewModel
{
    [ObservableProperty]
    private string _ipAddress;
    [ObservableProperty]
    private string _statusText;
    [ObservableProperty]
    private double _rotationAngle=0;
    [ObservableProperty]
    private string _deviceInfo;
    private bool _isConnected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(InitializeCommand))]
    private bool _isInitialized;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isStarted;
    [ObservableProperty]
    private string _currentAction = "Ready";
    public CircleMarkerGraph CirclePoints ;
    // [ObservableProperty]
    // public ObservableCollection<UIElement> charts = new ObservableCollection<UIElement>();

    private readonly Lms5XxConnector _connector = new Lms5XxConnector();
    public MainViewModel()
    {
#if DEBUG
        IpAddress = "192.168.1.231";
#endif
        _stopwatch.Start();
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

            MessageBox.Show("设备链接失败!"+e.Message);
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
            CurrentAction= "set stop Send data permanently success!";
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
            _connector.ReceivedHandles.AddOrUpdate((CommandTypes.Ssn, Commands.LMDscandata), handerScanData,
                (tuple, action) => handerScanData);
        }
        else
        {
            CurrentAction = "set Send data permanently failed!";
        }
        
    }

    private void handerScanData(TelegramContent telegramContent)
    {


        //在ui线程上执行
        Application.Current.Dispatcher.Invoke(() =>
        {
            Console.WriteLine("收到数据:" + telegramContent.Payload.CommandConnent.GetType());
            try
            {
                if (telegramContent.Payload.CommandConnent is LMDscandataModeCommand { OutputChannelList.Count: > 0 } dscandataModeCommand)
                {
                    ShowScanData(dscandataModeCommand);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
        
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


    private Stopwatch _stopwatch = new Stopwatch();
    void ShowScanData(LMDscandataModeCommand lmDscandataModeCommand)
    {
        var lmdScandata = lmDscandataModeCommand.OutputChannelList[0];
        // _traceWrapper.WriteInformation(Newtonsoft.Json.JsonConvert.SerializeObject(

        // //缩放倍率
        // var scale = 0.1;
        // DataCanvasViewModel.UpdateData(lmdScandata.DistDatas.Select((t, index) =>
        // {
        //     var dim = lmdScandata.ScaleFactor.Value * t.Value;
        //     var angle = ((double)(lmdScandata.StartAngle.Value) + index * (lmdScandata.AngularStepSize.Value)) /10000; // 角度，单位：度
        //     var point = new
        //     {
        //         X = dim * Math.Cos(angle * Math.PI / 180),
        //         Y = dim * Math.Sin(angle * Math.PI / 180),
        //         Dim = dim,
        //         // Color = ValueToColor(data.RSSI1.DataPoints[index],0, 65000, startColor, endCorol)
        //         Color = GetRssiColor(lmDscandataModeCommand.OutputChannel8BitList[0].DistDatas[index].Value)
        //     };
        //     return new ScanPoint(250 + point.X * scale, 300 - point.Y * scale, point.Dim, point.Color);
        // }));
        // var x = lmdScandata.DistDatas.Select((t, index) =>
        // {
        //     var dim = lmdScandata.ScaleFactor.Value * t.Value;
        //     var angle = ((double)(lmdScandata.StartAngle.Value) + index * (lmdScandata.AngularStepSize.Value)) / 10000; // 角度，单位：度
        //     return dim * Math.Cos(angle * Math.PI / 180);
        // });
        // var y = lmdScandata.DistDatas.Select((t, index) =>
        // {
        //     var dim = lmdScandata.ScaleFactor.Value * t.Value;
        //     var angle = ((double)(lmdScandata.StartAngle.Value) + index * (lmdScandata.AngularStepSize.Value)) / 10000; // 角度，单位：度
        //     return dim * Math.Sin(angle * Math.PI / 180);
        // });
        // var points = lmdScandata.DistDatas.Select((t, index) =>
        // {
        //     var dim = lmdScandata.ScaleFactor.Value * t.Value;
        //     var angle = ((double)(lmdScandata.StartAngle.Value) + index * (lmdScandata.AngularStepSize.Value)) / 10000; // 角度，单位：度
        //   angle += RotationAngle; // 应用旋转角度
        //     return new { X = dim * Math.Cos(angle * Math.PI / 180), Y = dim * Math.Sin(angle * Math.PI / 180), Dim = dim };
        // }).ToList();

        //计算每秒fps _stopwatch.Elapsed.TotalSeconds 
        var fps = 1000 / _stopwatch.Elapsed.TotalMicroseconds;
        _stopwatch.Restart();
        Console.WriteLine("ShowScanData:" + lmdScandata.DistDatas.Count + " ,OutputChannel8BitList:" + lmDscandataModeCommand.OutputChannel8BitList.Count
            + " ,fps:" + fps);



        for (var index = 0; index < lmdScandata.DistDatas.Count; index++)
        {
            var t= lmdScandata.DistDatas[index];
            var dim = lmdScandata.ScaleFactor.Value * t.Value;
            var angle = ((double)(lmdScandata.StartAngle.Value) + index * (lmdScandata.AngularStepSize.Value)) / 10000; // 角度，单位：度
            angle += RotationAngle; // 应用旋转角度
            // return new { X = dim * Math.Cos(angle * Math.PI / 180), Y = dim * Math.Sin(angle * Math.PI / 180), Dim = dim };
            Xs[index] = dim * Math.Cos(angle * Math.PI / 180);
            Ys[index] = dim * Math.Sin(angle * Math.PI / 180);
            // Cs[index] = GetRssiColor(lmDscandataModeCommand.OutputChannel8BitList[0].DistDatas[index].Value);
            Cs[index] = 255; //GetRssiColor(lmDscandataModeCommand.OutputChannel8BitList[0].DistDatas[index].Value);
        }
       

        // var x = points.Select(p => p.X);
        // var y = points.Select(p => p.Y);
        CirclePoints.PlotColor(
                Xs.AsSpan(0, lmdScandata.DistDatas.Count).ToArray(),
                Ys.AsSpan(0, lmdScandata.DistDatas.Count).ToArray(), 
                Cs.AsSpan(0, lmdScandata.DistDatas.Count).ToArray());

    }

    private double GetRssiColor(ushort value)
    {
        return value;
    }

    private double[] Xs = new double[1000];
    private double[] Ys = new double[1000];
    private double[] Cs = new double[1000];
    

}