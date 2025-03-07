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

        Assert.That(response?.Name, Is.EqualTo("LMS10x_FieldEval"));
        Assert.That(response?.Version, Is.EqualTo("V1.36-21.10.2010"));


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
                Assert.That(deviceInfo.NameLength, Is.EqualTo(0x10));
                Assert.That(deviceInfo.Name, Is.EqualTo("LMS10x_FieldEval"));

                // 验证固件版本长度和内容
                Assert.That(deviceInfo.VersionLength, Is.EqualTo(0x10));
                Assert.That(deviceInfo.Version, Is.EqualTo("V1.36-21.10.2010"));
            });

            TestContext.WriteLine("CoLa B 协议二进制反序列化测试通过");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"反序列化测试异常: {ex}");
            throw;
        }
    }

    [Test]
    public void TestDeserializeSEALMDscandata()
    {


        // 创建sEA LMDscandata命令对象
        var telegram = new CoLaB
        {
            Content = new TelegramContentB
            {
                CommandTypes = CommandTypes.Sea,
                Payload = new SenCommandContainerB
                {
                    Command = Commands.LMDscandata,
                    CommandConnent = new ConfirmationRequestModeCommandBaseB
                    {
                        StopStart = StopStartB.Start
                    }
                }
            }
        };

        // 序列化成二进制数据
        using var memoryStream = new MemoryStream();
        _serializer.Serialize(memoryStream, telegram);
        var actualBytes = memoryStream.ToArray();

        // 文档中的期望二进制数据
        var expectedHex = "02 02 02 02 00 00 00 11 73 45 41 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 01 3C";
        var expectedBytes = expectedHex.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();

        // 比较实际序列化结果与期望结果
        TestContext.WriteLine($"期望的字节数: {expectedBytes.Length}, 实际字节数: {actualBytes.Length}");
        TestContext.WriteLine($"期望的十六进制: {BitConverter.ToString(expectedBytes).Replace("-", " ")}");
        TestContext.WriteLine($"实际的十六进制: {BitConverter.ToString(actualBytes).Replace("-", " ")}");


        // 使用文档中的示例二进制数据 (CoLa B格式的 sEA LMDscandata)
        // 02 02 02 02 00 00 00 11 73 45 41 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 01 33
        var responseHex = "02 02 02 02 00 00 00 11 73 45 41 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 01 3C";
        var responseBytes = responseHex.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();

        // 创建内存流
        using var stream = new MemoryStream(responseBytes);

        // 反序列化
        var packet = _serializer.Deserialize<CoLaB>(stream);

        // 验证基本结构
        Assert.Multiple(() =>
        {
            // 验证命令类型
            Assert.That(packet.Content.CommandTypes, Is.EqualTo(CommandTypes.Sea));

            // 验证命令
            Assert.That(packet.Content.Payload.Command, Is.EqualTo(Commands.LMDscandata));

            // 验证是启动还是停止
            var confirmationCommand = packet.Content.Payload.CommandConnent as ConfirmationRequestModeCommandBaseB;
            Assert.That(confirmationCommand, Is.Not.Null);
            Assert.That(confirmationCommand.StopStart, Is.EqualTo(StopStartB.Start));
        });

        TestContext.WriteLine("反序列化 sEA LMDscandata 测试通过");

    }

    [Test]
    public void TestSerializeSENLMDscandata()
    {
        try
        {
            // 创建sEN LMDscandata命令对象
            var telegram = new CoLaB
            {
                Content = new TelegramContentB
                {
                    CommandTypes = CommandTypes.Sen,
                    Payload = new SenCommandContainerB
                    {
                        Command = Commands.LMDscandata,
                        CommandConnent = new ConfirmationRequestModeCommandBaseB
                        {
                            StopStart = StopStartB.Start
                        }
                    }
                }
            };

            // 序列化成二进制数据
            using var memoryStream = new MemoryStream();
            _serializer.Serialize(memoryStream, telegram);
            var actualBytes = memoryStream.ToArray();

            // 文档中的期望二进制数据
            // 02 02 02 02 00 00 00 11 73 45 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 01 33
            var expectedHex = "02 02 02 02 00 00 00 11 73 45 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 01 33";
            var expectedBytes = expectedHex.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();

            // 比较实际序列化结果与期望结果
            TestContext.WriteLine($"期望的字节数: {expectedBytes.Length}, 实际字节数: {actualBytes.Length}");
            TestContext.WriteLine($"期望的十六进制: {BitConverter.ToString(expectedBytes).Replace("-", " ")}");
            TestContext.WriteLine($"实际的十六进制: {BitConverter.ToString(actualBytes).Replace("-", " ")}");

            // 验证长度和内容
            Assert.That(actualBytes.Length, Is.EqualTo(expectedBytes.Length), "序列化后的数据长度与期望不符");

            // 逐字节比较
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                if (actualBytes[i] != expectedBytes[i])
                {
                    TestContext.WriteLine($"不匹配的位置: {i}, 期望: 0x{expectedBytes[i]:X2}, 实际: 0x{actualBytes[i]:X2}");
                }
            }

            Assert.That(actualBytes, Is.EqualTo(expectedBytes), "序列化后的数据内容与期望不符");

            TestContext.WriteLine("sEN LMDscandata 序列化测试通过");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"测试失败: {ex.Message}");
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    public void TestDeserializeLMDscandataB()
    {
        // 使用文档中的示例二进制数据 (CoLa B格式的 sRA LMDscandata)
        // 这里使用简化的示例数据，实际应用中应使用完整的扫描数据
        var responseHex = """
                          02 02 02 02 00 00 00 83 73 52 41 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 00 01 00 01 00 89
                          A2 7F 00 00 03 43 03 47 27 47 7B A9 27 47 81 3B 00 00 07 00 00 00 00 00 13 88 00 00 01 68
                          00 00 00 01 44 49 53 54 31 3F 80 00 00 00 00 00 00 00 01 86 A0 13 88 00 15 08 93 08 95 08
                          AF 08 B3 08 B0 08 A4 08 B0 08 BF 08 B9 08 BA 08 D0 08 D3 08 CF 08 DE 08 EB 08 E3 08 FE 08
                          EC 09 03 08 FD 08 FD 00 00 00 00 00 00 00 00 00 00 00 00 2B
                          """;

        try
        {
            // 清理并转换十六进制字符串为字节数组
            var cleanedHex = responseHex
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(hex => hex.Trim())
                .Where(hex => !string.IsNullOrWhiteSpace(hex) && hex.Length == 2) // 确保每个十六进制字符串都是2位
                .ToArray();

            var responseBytes = new byte[cleanedHex.Length];
            for (int i = 0; i < cleanedHex.Length; i++)
            {
                if (!byte.TryParse(cleanedHex[i], System.Globalization.NumberStyles.HexNumber, null, out responseBytes[i]))
                {
                    throw new FormatException($"无法将十六进制字符串 '{cleanedHex[i]}' 转换为字节，位置: {i}");
                }
            }

            TestContext.WriteLine($"成功转换 {responseBytes.Length} 个字节");
            TestContext.WriteLine($"转换后的十六进制: {BitConverter.ToString(responseBytes).Replace("-", " ")}");

            // 创建内存流
            using var stream = new MemoryStream(responseBytes);

            // 反序列化
            var packet = _serializer.Deserialize<CoLaB>(stream);

            // 验证基本结构
            Assert.Multiple(() =>
            {
                // 验证命令类型
                Assert.That(packet.Content.CommandTypes, Is.EqualTo(CommandTypes.Sra));

                // 验证命令
                Assert.That(packet.Content.Payload.Command, Is.EqualTo(Commands.LMDscandata));

                // 获取扫描数据内容
                var scanData = packet.Content.Payload.CommandConnent as LMDscandataModeCommandB;
                Assert.That(scanData, Is.Not.Null);

                // 验证扫描数据的基本字段
                Assert.That(scanData.VersionNumber, Is.EqualTo(1));
                Assert.That(scanData.DeviceNumber, Is.EqualTo(1));
                Assert.That(scanData.SerialNumber, Is.EqualTo(9020031));
                Assert.That(scanData.TelegramCounter, Is.EqualTo(835));
                Assert.That(scanData.ScanCounter, Is.EqualTo(839));

                // 验证扫描频率
                Assert.That(scanData.ScanFrequency, Is.EqualTo(5000)); // 50Hz

                // 验证16位通道数量
                Assert.That(scanData.AmountOf16BitChannels, Is.EqualTo(1));

                // 验证起始角度
                Assert.That(scanData.StartAngle, Is.EqualTo(100000)); // 10°

                // 验证角步长
                Assert.That(scanData.AngularStepSize, Is.EqualTo(5000)); // 0.5°

                // 验证数据量
                Assert.That(scanData.AmountOfData, Is.EqualTo(21));

                // 验证数据数组不为空
                Assert.That(scanData.Data, Is.Not.Null);
                Assert.That(scanData.Data.Length, Is.EqualTo(21));
            });

            TestContext.WriteLine("LMDscandataModeCommandB 反序列化测试通过");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"反序列化测试异常: {ex}");
            throw;
        }
    }
}