using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using LMS5xxConnector;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Telegram.CommandContents;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace DistanceSensorAppDemo;

public class MainViewModel : INotifyPropertyChanged
{
    private string _ipAddress;
    private bool _isConnected;
    private bool _isInitialized;
    private bool _isStarted;
    private bool _deviceInfoVisibility;
    private string _statusText;
    private string _deviceInfo;

    public MainViewModel()
    {
        ConnectCommand = new RelayCommand(Connect);
        DisconnectCommand = new RelayCommand(Disconnect);
        InitializeCommand = new RelayCommand(Initialize, CanInitialize);
        StartCommand = new RelayCommand(Start, CanStart);
        GetDataCommand = new RelayCommand(GetData);
        StopCommand = new RelayCommand(Stop, CanStop);
        RestartCommand = new RelayCommand(Restart);
        LoginCommand = new RelayCommand(Login);
        GetDeviceInfoCommand = new RelayCommand(GetDeviceInfo);
        DataCanvasViewModel = new DataCanvasViewModel();
        IpAddress = "192.168.0.231";
    }


    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged(nameof(IpAddress));
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsDisconnected));
            if (IsConnected)
            {
                StatusText = "Connected";
            }
            else
            {
                StatusText = "Disconnected";
            }
            // InitializeCommand.RaiseCanExecuteChanged();
            // RestartCommand.RaiseCanExecuteChanged();
            // LoginCommand.RaiseCanExecuteChanged();
            // GetDeviceInfoCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsDisconnected => !IsConnected;

    public bool IsInitialized
    {
        get => _isInitialized;
        set
        {
            _isInitialized = value;
            OnPropertyChanged(nameof(IsInitialized));
            // StartCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsStarted
    {
        get => _isStarted;
        set
        {
            _isStarted = value;
            OnPropertyChanged(nameof(IsStarted));
            // StopCommand.RaiseCanExecuteChanged();
        }
    }


    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string DeviceInfo
    {
        get => _deviceInfo;
        set
        {
            _deviceInfo = value;
            OnPropertyChanged(nameof(DeviceInfo));
        }
    }

    public RelayCommand ConnectCommand { get; }

    public RelayCommand DisconnectCommand { get; }

    public RelayCommand InitializeCommand { get; }

    public RelayCommand StartCommand { get; }
    public RelayCommand GetDataCommand { get; }

    public RelayCommand StopCommand { get; }

    public RelayCommand RestartCommand { get; }

    public RelayCommand LoginCommand { get; }

    public RelayCommand GetDeviceInfoCommand { get; }

    public DataCanvasViewModel DataCanvasViewModel { get; }

    public string CurrentAction
    {
        get => _currentAction;
        set
        {
            if (value == _currentAction) return;
            _currentAction = value;
            OnPropertyChanged(nameof(CurrentAction));
        }
    }

    private async void Connect()
    {
        if (string.IsNullOrEmpty(_ipAddress))
        {
            MessageBox.Show("ip地址错误!");
            return;
        }

        try
        {
            _connector.ConnectAsync(_ipAddress + ":2111");
        }
        catch (FormatException e)
        {
            MessageBox.Show("ip地址格式错误!");
            return;
        }

        IsConnected = _connector.IsConnected;
    }

    private Lms5XxConnector _connector = new Lms5XxConnector();
    private string _currentAction = "Ready";

    private async void Disconnect()
    {
        // TODO: Implement disconnect logic
        // Set IsConnected to false
        _connector.Disconnect();

        IsConnected = _connector.IsConnected;

        IsConnected = false;
        IsInitialized = false;
        IsStarted = false;

        DataCanvasViewModel.ClearData();
    }

    private bool CanInitialize()
    {
        return IsConnected && !IsInitialized;
    }

    private async void Initialize()
    {
        // TODO: Implement initialization logic
        // Set IsInitialized to true if initialization is successful
        IsInitialized = true;
    }

    private bool CanStart()
    {
        return IsConnected  && !IsStarted;
    }

    

    private bool CanStop()
    {
        return IsConnected; //&& IsStarted;
    }

    private async void Stop()
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

    private async void Restart()
    {
        // TODO: Implement restart logic
        // This should stop the acquisition, reinitialize the device, and start the acquisition again
    }

    private async void Login()
    {
        // TODO: Implement login logic
        // This should prompt the user for credentials and perform a login
    }

    private async void GetDeviceInfo()
    {
        // TODO: Implement get device info logic
        // This should retrieve device information and display it in DeviceInfo
        CurrentAction = "start GetDeviceInfo";
        var uniqueIdentificationModeCommand = await _connector.GetDeviceInfo();
        this.DeviceInfo = uniqueIdentificationModeCommand?.Name + "   " + uniqueIdentificationModeCommand?.Version;
        CurrentAction = "Ready";
    }

    private async void Start()
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
            if (telegramContent.Payload.CommandConnent is LMDscandataModeCommand { OutputChannelList.Count: > 0 } dscandataModeCommand)
            {
                ShowScanData(dscandataModeCommand);
            }
        });
        
        // if ( telegramContent.Payload.CommandConnent is LMDscandataModeCommand { OutputChannelList.Count: > 0 } dscandataModeCommand )
        // {
        //     ShowScanData(dscandataModeCommand);
        // }
    }

    private async void GetData()
    {
  
        CurrentAction = "start GetData";
        var dscandataModeCommand = await _connector.GetScanData();

        if (dscandataModeCommand != null && dscandataModeCommand.OutputChannelList.Count > 0)
        {
            ShowScanData(dscandataModeCommand);
        }

        CurrentAction = "Ready";
    }  
    void ShowScanData(LMDscandataModeCommand lmDscandataModeCommand)
    {
        var lmdScandata = lmDscandataModeCommand.OutputChannelList[0];
        // _traceWrapper.WriteInformation(Newtonsoft.Json.JsonConvert.SerializeObject(


        //缩放倍率
        var scale = 0.1;

        DataCanvasViewModel.UpdateData(lmdScandata.DistDatas.Select((t, index) =>
        {
            var dim = lmdScandata.ScaleFactor.Value * t.Value;
            var angle = ((double)(lmdScandata.StartAngle.Value) + index * (lmdScandata.AngularStepSize.Value)) /
                        10000; // 角度，单位：度

            var point = new
            {
                X = dim * Math.Cos(angle * Math.PI / 180),
                Y = dim * Math.Sin(angle * Math.PI / 180),
                Dim = dim,
                // Color = ValueToColor(data.RSSI1.DataPoints[index],0, 65000, startColor, endCorol)
                Color = GetRssiColor(lmDscandataModeCommand.OutputChannel8BitList[0].DistDatas[index].Value)
            };
            return new ScanPoint(250 + point.X * scale, 300 - point.Y * scale, point.Dim, point.Color);
        }));
    }


    public Color GetRssiColor(double uIntPtr)
    {
        Color temp = new Color();

        double GetColorFactor(double val, double scale)
        {
            return val * uIntPtr - val * scale;
        }

        if (uIntPtr < 10) //  Black -> Blue Section
        {
            temp = Color.FromArgb(255, 0, 0, (byte)((double)28.3 * (double)uIntPtr));
        }
        else if ((10 <= uIntPtr) && (uIntPtr < 30)) //  Blue -> Teal Section
        {
            double r = GetColorFactor(127 / (29 - 10), 10);
            temp = Color.FromArgb(255, 0, (byte)r, 255 - (byte)r);
        }
        else if ((30 <= uIntPtr) && (uIntPtr < 50)) //  Teal -> Cyan Section
        {
            double r = GetColorFactor(127 / (49 - 30), 30);
            temp = Color.FromArgb(255, 0, 128 + (byte)r, 128 + (byte)r);
        }
        else if ((50 <= uIntPtr) && (uIntPtr < 70)) //  Cyan -> Spring Green Section
        {
            double r = GetColorFactor(127 / (69 - 50), 50);
            temp = Color.FromArgb(255, 0, 255, 255 - (byte)r);
        }
        else if ((70 <= uIntPtr) && (uIntPtr < 90)) // Spring green -> Lime Section
        {
            double r = GetColorFactor(127 / (89 - 70), 70);
            temp = Color.FromArgb(255, 0, 255, 127 - (byte)r);
        }
        else if ((90 <= uIntPtr) && (uIntPtr < 110)) // Lime -> Yellow Section
        {
            double r = GetColorFactor(255 / (109 - 90), 90);
            temp = Color.FromArgb(255, (byte)r, 255, 0);
        }
        else if ((110 <= uIntPtr) && (uIntPtr < 140)) // Yellow -> Orange Section
        {
            double r = GetColorFactor(90 / (139 - 110), 110);
            temp = Color.FromArgb(255, 255, 255 - (byte)r, 0);
        }
        else if ((140 <= uIntPtr) && (uIntPtr < 170)) // Orange -> Red Section
        {
            double r = GetColorFactor(165 / (169 - 140), 140);
            temp = Color.FromArgb(255, 255, 165 - (byte)r, 0);
        }
        else if (170 <= uIntPtr)
        {
            temp = Color.FromArgb(255, 255, 0, 0); //   red
        }

        return temp;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}