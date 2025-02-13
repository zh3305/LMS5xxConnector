using System.Text;
using BinarySerialization;
using LMS5xxConnector;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Telegram.CommandContainers;
using LMS5xxConnector.Telegram.CommandContents;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;
using System.Collections.Generic;

namespace LMS5xxConnectorTest;

public class CoLaAUnitTest
{
    private Lms5XxConnector _connector;
    private TcpListener _tcpServer;
    private TcpClient _client;
    private bool _isServerRunning;
    private bool _isScanning = false;
    private CancellationTokenSource _scanCts = new CancellationTokenSource();
    private BinarySerializer _serializer;

    [SetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Trace)
                .AddNUnit();
        });
        _connector = new Lms5XxConnector(loggerFactory.CreateLogger<Lms5XxConnector>());

        // 启动模拟服务器
        _tcpServer = new TcpListener(IPAddress.Any, 2111);
        _tcpServer.Start();
        _isServerRunning = true;

        // 在后台线程处理客户端连接
        Task.Run(async () =>
        {
            try
            {
                while (_isServerRunning)
                {
                    if (_tcpServer.Pending())
                    {
                        _client = await _tcpServer.AcceptTcpClientAsync();
                        HandleClientCommunication(_client);
                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"服务器异常: {ex.Message}");
            }
        });

        _serializer = new BinarySerializer();
    }

    private void HandleClientCommunication(TcpClient client)
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
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            TestContext.WriteLine($"收到数据: {received}");

                            if (received.Contains("sRN DeviceIdent"))
                            {
                                // 模拟设备响应
                                // 格式: <STX> sRA {SPC} DeviceIdent {SPC} 10 {SPC} LMS10x_FieldEval {SPC} 10 {SPC} V1.36-21.10.2010 <ETX>
                                string response = "\x02sRA DeviceIdent 10 LMS10x_FieldEval 10 V1.36-21.10.2010\x03";
                                byte[] responseData = Encoding.ASCII.GetBytes(response);
                                await stream.WriteAsync(responseData, 0, responseData.Length);
                                TestContext.WriteLine($"发送响应: {response}");
                            }
                            else if (received.Contains("sEN LMDscandata"))
                            {
                                if (received.Contains("1")) // 启动扫描
                                {
                                    _isScanning = true;
                                    _scanCts = new CancellationTokenSource();

                                    // 首先发送确认响应
                                    string response = "\x02sEA LMDscandata 1\x03";
                                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                                    await stream.WriteAsync(responseData, 0, responseData.Length);
                                    TestContext.WriteLine($"发送启动确认: {response}");

                                    // 然后开始发送扫描数据
                                    StartScanDataSimulation(stream, _scanCts.Token);
                                }
                                else if (received.Contains("0")) // 停止扫描
                                {
                                    _isScanning = false;
                                    _scanCts.Cancel();
                                    string response = "\x02sEA LMDscandata 0\x03";
                                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                                    await stream.WriteAsync(responseData, 0, responseData.Length);
                                    TestContext.WriteLine($"发送停止确认: {response}");
                                }
                            }
                        }
                    }
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"客户端处理异常: {ex.Message}");
            }
        });
    }

    private void StartScanDataSimulation(NetworkStream stream, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            try
            {
                int scanCounter = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 构建标准格式的扫描数据响应
                    // var response = new StringBuilder("\x02sRA LMDscandata");
                    //
                    // // 协议版本和设备编号
                    // response.Append(" 1 1");
                    // // 序列号
                    // response.Append(" 89A27F");
                    // // 设备状态和附加状态
                    // response.Append(" 0 0");
                    // // 报文计数器
                    // response.Append(" 343");
                    // // 扫描计数器(递增)
                    // response.AppendFormat(" {0}", scanCounter++);
                    // // 时间戳
                    // response.Append(" 27477BA9 2747813B");
                    // // 输入状态
                    // response.Append(" 0 0");
                    // // 输出状态
                    // response.Append(" 3F 0");
                    // // 保留字段
                    // response.Append(" 0");
                    // // 编码器值和速度值
                    // response.Append(" 1388 168");
                    // // 通道数和激活通道
                    // response.Append(" 0 1");
                    // // 通道内容
                    // response.Append(" DIST1");
                    // // 缩放因子和偏移
                    // response.Append(" 3F800000 00000000");
                    // // 起始角度和角度分辨率
                    // response.Append(" 186A0 1388");
                    // // 数据点数
                    // response.Append(" 15");
                    //
                    // // 标准测试数据点
                    // string[] sampleData = {
                    //     "8A1", "8A5", "8AB", "8AC", "8A6", "8AC", 
                    //     "8B6", "8C8", "8C2", "8C9", "8CB", "8C4",
                    //     "8E4", "8E1", "8EB", "8E0", "8F5", "908",
                    //     "8FC", "907", "906"
                    // };
                    //
                    // // 添加数据点
                    // foreach (var data in sampleData)
                    // {
                    //     response.AppendFormat(" {0}", data);
                    // }
                    //
                    // // 补充剩余的0值
                    // response.Append(" 0 0 0 0 0 0");
                    //
                    // // 添加结束符
                    // response.Append("\x03");
                    //
                    // byte[] responseData = Encoding.ASCII.GetBytes(response.ToString());
                    //<STX>sRA{SPC}LMDscandata{SPC}1{SPC}1{SPC}89A27F{SPC}0{SPC}0{SPC}343{SPC}347{SPC}27477BA9{SPC}2747813B{SPC}0{SPC}0{SPC}7{SPC}0{SPC}0{SPC}1388{SPC}168{SPC}0{SPC}1{SPC}DIST1{SPC}3F800000{SPC}00000000{SPC}186A0{SPC}1388{SPC}15{SPC}8A1{SPC}8A5{SPC}8AB{SPC}8AC{SPC}8A6{SPC}8AC{SPC}8B6{SPC}8C8{SPC}8C2{SPC}8C9{SPC}8CB{SPC}8C4{SPC}8E4{SPC}8E1{SPC}8EB{SPC}8E0{SPC}8F5{SPC}908{SPC}8FC{SPC}907{SPC}906{SPC}0{SPC}0{SPC}0{SPC}0{SPC}0{SPC}0<ETX>
                    string response = "\x02sRA LMDscandata 1 1 89A27F 0 0 343 347 27477BA9 2747813B 0 0 7 0 0 1388 168 0 1 DIST1 3F800000 00000000 186A0 1388 15 8A1 8A5 8AB 8AC 8A6 8AC 8B6 8C8 8C2 8C9 8CB 8C4 8E4 8E1 8EB 8E0 8F5 908 8FC 907 906 0 0 0 0 0 0\x03";
                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    
                    await Task.Delay(100); // 保持10Hz频率
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"扫描模拟异常: {ex.Message}");
            }
        }, cancellationToken);
    }


    [TearDown]
    public void TearDown()
    {
        _isServerRunning = false;
        _client?.Close();
        _tcpServer?.Stop();
    }

    [Test]
    public async Task TestDeviceIdent()
    {
        // 连接到模拟服务器
        await _connector.ConnectAsync("127.0.0.1:2111");

        // 发送DeviceIdent命令并验证响应
        var response = await _connector.GetDeviceInfo();
        TestContext.Out.WriteLine(response.Name);
        TestContext.Out.WriteLine(response.Version);
    }

    public void Test2()
    {


    }

    [Test]
    public void Test1()
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
        var packet = new CoLaA
        {
            Content = telegram
        };

        var serializer = new BinarySerializer();
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, packet);
        var array = stream.ToArray();
        TestContext.Out.WriteLine(Encoding.ASCII.GetString(array));
        TestContext.Out.WriteLine(BitConverter.ToString(array).Replace("-", ""));


        // Assert.Pass();
    }

    [Test]
    public async Task TestStartScanData()
    {
        try
        {
            await _connector.ConnectAsync("127.0.0.1:2111");

            var receivedDataCount = 0;
            var scanDataReceived = new TaskCompletionSource<bool>();

            _connector.ReceivedHandles.TryAdd(
                (CommandTypes.Sra, Commands.LMDscandata),
                (telegramContent) =>
                {
                    TestContext.WriteLine($"收到扫描数据: {telegramContent}");
                    // 修改枚举值处理方式
                    if (telegramContent.Payload.CommandConnent is LMDscandataModeCommand scanData)
                    {
                        receivedDataCount++;
                        if (receivedDataCount >= 5)
                        {
                            scanDataReceived.TrySetResult(true);
                        }
                    }
                });

            var result = await _connector.StartScanData();

            Assert.That(result, Is.EqualTo(StopStart.Start), "扫描启动应返回Start状态");
            TestContext.WriteLine("扫描启动应返回Start状态:"+ result);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(scanDataReceived.Task, timeoutTask);

            Assert.That(completedTask, Is.EqualTo(scanDataReceived.Task), "应该在超时前收到足够的扫描数据");
            Assert.That(receivedDataCount, Is.GreaterThanOrEqualTo(5), "应该收到至少5帧扫描数据");

            result = await _connector.StopScanData();
            Assert.That(result, Is.EqualTo(StopStart.Stop), "扫描停止应返回Stop状态");
        }
        finally
        {
            _connector.ReceivedHandles.TryRemove((CommandTypes.Sra, Commands.LMDscandata), out _);
            _connector.Disconnect();
        }
    }

    [Test]
    public void TestDeserializeLMDscandata()
    {
        try
        {
            // 构造标准的扫描数据响应
            string response = "\x02sRA LMDscandata 1 1 89A27F 0 0 343 347 27477BA9 2747813B 0 0 7 0 0 1388 168 0 1 DIST1 3F800000 00000000 186A0 1388 15 8A1 8A5 8AB 8AC 8A6 8AC 8B6 8C8 8C2 8C9 8CB 8C4 8E4 8E1 8EB 8E0 8F5 908 8FC 907 906 0 0 0 0 0 0\x03";
            byte[] responseData = Encoding.ASCII.GetBytes(response);

            // 创建内存流
            using var stream = new MemoryStream(responseData);
            
            // 反序列化
            var packet = _serializer.Deserialize<CoLaA>(stream);
            
            // 验证基本结构
            Assert.That(packet, Is.Not.Null);
            Assert.That(packet.Content, Is.Not.Null);
            Assert.That(packet.Content.CommandTypes, Is.EqualTo(CommandTypes.Sra));
            Assert.That(packet.Content.Payload, Is.Not.Null);
            
            // 获取扫描数据内容
            var scanData = packet.Content.Payload.CommandConnent as LMDscandataModeCommand;
            Assert.That(scanData, Is.Not.Null);
            
            // // 验证具体字段
            // Assert.Multiple(() =>
            // {
            //     // 基本信息
            //     Assert.That(scanData.Versionnumber, Is.EqualTo("1"));
            //     Assert.That(scanData.DeviceNumber, Is.EqualTo("1"));
            //     Assert.That(scanData.SerialNumber, Is.EqualTo("89A27F"));
            //     
            //     // 状态信息
            //     Assert.That(scanData.DeviceStatus, Is.EqualTo(DeviceStatusEnum.Ok));
            //     Assert.That(scanData.TelegramCounter.Value, Is.EqualTo(343));
            //     Assert.That(scanData.ScanCounter.Value, Is.EqualTo(347));
            //     
            //     // 时间戳
            //     Assert.That(scanData.TimeSinceStartup.Value, Is.EqualTo(0x27477BA9));
            //     Assert.That(scanData.TimeOfTransmission.Value, Is.EqualTo(0x2747813B));
            //     
            //     // 输入输出状态
            //     Assert.That(scanData.InputStatus, Is.EqualTo("0"));
            //     Assert.That(scanData.InputStatus2, Is.EqualTo("0"));
            //     Assert.That(scanData.OutputStatus, Is.EqualTo("3F"));
            //     Assert.That(scanData.OutputStatus2, Is.EqualTo("0"));
            //     
            //     // 扫描配置
            //     Assert.That(scanData.ScanFrequency, Is.EqualTo("1388"));
            //     Assert.That(scanData.MeasurementFrequency, Is.EqualTo("168"));
            //     
            //     // 通道信息
            //     Assert.That(scanData.Channel16BitNumber.Value, Is.EqualTo(1));
            //     Assert.That(scanData.OutputChannelList, Has.Count.EqualTo(1));
            //     Assert.That(scanData.OutputChannelList[0].Content, Is.EqualTo("DIST1"));
            //     Assert.That(scanData.OutputChannelList[0].ScalingFactor, Is.EqualTo(0x3F800000));
            //     Assert.That(scanData.OutputChannelList[0].ScalingOffset, Is.EqualTo(0x00000000));
            //     Assert.That(scanData.OutputChannelList[0].StartAngle, Is.EqualTo(0x186A0));
            //     Assert.That(scanData.OutputChannelList[0].AngularResolution, Is.EqualTo(0x1388));
            //     Assert.That(scanData.OutputChannelList[0].DataCount, Is.EqualTo(15));
            // });

            TestContext.WriteLine("反序列化测试通过");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"反序列化测试异常: {ex}");
            throw;
        }
    }
}