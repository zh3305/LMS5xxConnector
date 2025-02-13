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
using System.IO;
using System.Linq;

namespace LMS5xxConnectorTest;

public class CoLaBUnitTest
{
    private Lms5XxConnectorB _connector;
    private TcpListener _tcpServer;
    private TcpClient _client;
    private bool _isServerRunning;
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
        _connector = new Lms5XxConnectorB(loggerFactory.CreateLogger<Lms5XxConnectorB>());

        _tcpServer = new TcpListener(IPAddress.Any, 2111);
        _tcpServer.Start();
        _isServerRunning = true;

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
        _serializer.Endianness = Endianness.Big;
        _serializer.MemberSerializing += OnMemberSerializing;
        _serializer.MemberSerialized += OnMemberSerialized;
        _serializer.MemberDeserializing += OnMemberDeserializing;
        _serializer.MemberDeserialized += OnMemberDeserialized;

    }

    private static void OnMemberSerializing(object sender, MemberSerializingEventArgs e)
    {
        TestContext.WriteLine($"S-Start: {e.MemberName} @ {e.Offset}");
    }

    private static void OnMemberSerialized(object sender, MemberSerializedEventArgs e)
    {
        string hexValue = "";
        if (e.Value != null)
        {
            if (e.Value is byte[] bytes)
            {
                hexValue = BitConverter.ToString(bytes).Replace("-", " ");
            }
            else if (e.Value is string str)
            {
                hexValue = BitConverter.ToString(Encoding.ASCII.GetBytes(str)).Replace("-", " ");
            }
            else if (e.Value is CommandTypes cmdType)
            {
                hexValue = BitConverter.ToString(Encoding.ASCII.GetBytes(cmdType.ToString())).Replace("-", " ");
            }
            else if (e.Value is Commands cmd)
            {
                hexValue = BitConverter.ToString(Encoding.ASCII.GetBytes(cmd.ToString())).Replace("-", " ");
            }
            else if (e.Value is ushort || e.Value is uint)
            {
                hexValue = Convert.ToString(Convert.ToInt32(e.Value), 16).PadLeft(2, '0').ToUpper();
            }
            else
            {
                hexValue = e.Value.ToString();
            }
        }

        TestContext.WriteLine($"S-End: {e.MemberName} ({hexValue}) @ {e.Offset}");
    }

    private static void OnMemberDeserializing(object sender, MemberSerializingEventArgs e)
    {
        TestContext.WriteLine($"D-Start: {e.MemberName} @ {e.Offset}");
    }

    private static void OnMemberDeserialized(object sender, MemberSerializedEventArgs e)
    {
        string hexValue = "";
        if (e.Value != null)
        {
            if (e.Value is byte[] bytes)
            {
                hexValue = BitConverter.ToString(bytes).Replace("-", " ");
            }
            else if (e.Value is string str)
            {
                hexValue = BitConverter.ToString(Encoding.ASCII.GetBytes(str)).Replace("-", " ");
            }
            else if (e.Value is CommandTypes cmdType)
            {
                hexValue = BitConverter.ToString(Encoding.ASCII.GetBytes(cmdType.ToString())).Replace("-", " ");
            }
            else if (e.Value is Commands cmd)
            {
                hexValue = BitConverter.ToString(Encoding.ASCII.GetBytes(cmd.ToString())).Replace("-", " ");
            }
            else if (e.Value is ushort || e.Value is uint)
            {
                hexValue = Convert.ToString(Convert.ToInt32(e.Value), 16).PadLeft(2, '0').ToUpper();
            }
            else
            {
                hexValue = e.Value.ToString();
            }
        }

        TestContext.WriteLine($"D-End: {e.MemberName} ({hexValue}) @ {e.Offset}");
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
                        var request = await _serializer.DeserializeAsync<CoLaB>(stream);
                        
                        TestContext.WriteLine($"收到请求: {request.Content.CommandTypes} {request.Content.Payload.Command}");
                        if (request.Content is { CommandTypes: CommandTypes.Srn, Payload.Command: Commands.DeviceIdent })
                        {
                            TestContext.WriteLine($"收到设备标识请求:{request.Content.Payload.Command}");
                            // 02 02 02 02 00 00 00 34 73 52 41 20 44 65 76 69 63 65 49 64 65 6E 74 20 
                            // 00 10 4C 4D 53 31 30 78 5F 46 69 65 6C 64 45 76 61 6C 
                            // 00 10 56 31 2E 33 36 2D 32 31 2E 31 30 2E 32 30 31 30 62
                            // var response = new CoLaB
                            // {
                            //     Content = new TelegramContentB
                            //     {
                            //         CommandTypes = CommandTypes.Sra,
                            //         Payload = new SraCommandContainerB
                            //         {
                            //             Command = Commands.DeviceIdent,
                            //             CommandConnent = new UniqueIdentificationModeCommandB
                            //             {
                            //                 DeviceDesignationLength = 0x10,
                            //                 DeviceDesignation = "LMS10x_FieldEval",
                            //                 FirmwareVersionLength = 0x10,
                            //                 FirmwareVersion = "V1.36-21.10.2010"
                            //             }
                            //         }
                            //     }
                            // };
                            //
                            // await _serializer.SerializeAsync(stream, response);

                            var responseHex = "02 02 02 02 00 00 00 34 73 52 41 20 44 65 76 69 63 65 49 64 65 6E 74 20 00 10 4C 4D 53 31 30 78 5F 46 69 65 6C 64 45 76 61 6C 00 10 56 31 2E 33 36 2D 32 31 2E 31 30 2E 32 30 31 30 62";
                            var responseBytes = responseHex.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();

                            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        }

                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"客户端处理异常: {ex.Message}");
            }
        });
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
        await _connector.ConnectAsync("127.0.0.1:2111");

        var response = await _connector.GetDeviceInfo();
        
        Assert.That(response?.DeviceDesignation, Is.EqualTo("LMS10x_FieldEval"));
        Assert.That(response?.FirmwareVersion, Is.EqualTo("V1.36-21.10.2010"));

        
    }

    [Test]
    public void TestDeserializeDeviceIdent()
    {
        try
        {
            // 使用文档中的示例二进制数据
            var responseHex = "02 02 02 02 00 00 00 34 73 52 41 20 44 65 76 69 63 65 49 64 65 6E 74 20 00 10 4C 4D 53 31 30 78 5F 46 69 65 6C 64 45 76 61 6C 00 10 56 31 2E 33 36 2D 32 31 2E 31 30 2E 32 30 31 30 62";
            var responseBytes = responseHex.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();

            // 创建内存流
            using var stream = new MemoryStream(responseBytes);
            
            // 反序列化
            var packet = _serializer.Deserialize<CoLaB>(stream);
            
            // 验证基本结构
            Assert.Multiple(() =>
            {
                // 验证STX
                Assert.That(packet.STX, Is.EqualTo(new byte[] { 0x02, 0x02, 0x02, 0x02 }));
                
                // 验证长度
                Assert.That(packet.Length, Is.EqualTo(0x34));
                
                // 验证命令类型
                Assert.That(packet.Content.CommandTypes, Is.EqualTo(CommandTypes.Sra));
                
                // 验证命令
                Assert.That(packet.Content.Payload.Command, Is.EqualTo(Commands.DeviceIdent));
                
                // 获取设备信息内容
                var deviceInfo = packet.Content.Payload.CommandConnent as UniqueIdentificationModeCommandB;
                Assert.That(deviceInfo, Is.Not.Null);
                
                // 验证设备标识长度和内容
                Assert.That(deviceInfo.DeviceDesignationLength, Is.EqualTo(0x10));
                Assert.That(deviceInfo.DeviceDesignation, Is.EqualTo("LMS10x_FieldEval"));
                
                // 验证固件版本长度和内容
                Assert.That(deviceInfo.FirmwareVersionLength, Is.EqualTo(0x10));
                Assert.That(deviceInfo.FirmwareVersion, Is.EqualTo("V1.36-21.10.2010"));
            });

            TestContext.WriteLine("CoLa B 协议二进制反序列化测试通过");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"反序列化测试异常: {ex}");
            throw;
        }
    }
} 