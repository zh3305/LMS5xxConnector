using System.Diagnostics;
using System.Text;
using BinarySerialization;
using LMS5xxConnector.Telegram;
using LMS5xxConnector.Telegram.CommandContents;
using NUnit.Framework;

namespace LMS5xxConnectorTest;

public class CoLaPerformanceTest
{
    private BinarySerializer _serializer;
    private byte[] _colaAData;
    private byte[] _colaBData;
    private const int IterationCount = 10000;

    [SetUp]
    public void Setup()
    {
        _serializer = new BinarySerializer { Endianness = Endianness.Big };

        // 准备 CoLa A 测试数据
        string colaAResponse = "\x02sRA DeviceIdent 10 LMS10x_FieldEval 10 V1.36-21.10.2010\x03";
        _colaAData = Encoding.ASCII.GetBytes(colaAResponse);

        // 准备 CoLa B 测试数据
        var colaBHex = "02 02 02 02 00 00 00 34 73 52 41 20 44 65 76 69 63 65 49 64 65 6E 74 20 00 10 4C 4D 53 31 30 78 5F 46 69 65 6C 64 45 76 61 6C 00 10 56 31 2E 33 36 2D 32 31 2E 31 30 2E 32 30 31 30 62";
        _colaBData = colaBHex.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
    }

    [Test]
    public void CompareDeserializationPerformance()
    {
        var stopwatch = new Stopwatch();
        var results = new Dictionary<string, long>();


        // 测试 CoLa B
        stopwatch.Restart();
        for (int i = 0; i < IterationCount; i++)
        {
            using var stream = new MemoryStream(_colaBData);
            var packet = _serializer.Deserialize<CoLaB>(stream);
        }
        results["CoLa B"] = stopwatch.ElapsedMilliseconds;

        
        // 测试 CoLa A
        stopwatch.Restart();
        for (int i = 0; i < IterationCount; i++)
        {
            using var stream = new MemoryStream(_colaAData);
            var packet = _serializer.Deserialize<CoLaA>(stream);
        }
        results["CoLa A"] = stopwatch.ElapsedMilliseconds;

        // 输出结果
        TestContext.WriteLine($"性能测试结果 (迭代次数: {IterationCount}):");
        TestContext.WriteLine($"CoLa A 总耗时: {results["CoLa A"]}ms, 平均每次: {results["CoLa A"] / (double)IterationCount:F4}ms");
        TestContext.WriteLine($"CoLa B 总耗时: {results["CoLa B"]}ms, 平均每次: {results["CoLa B"] / (double)IterationCount:F4}ms");
        TestContext.WriteLine($"性能比率 (CoLa B / CoLa A): {results["CoLa B"] / (double)results["CoLa A"]:F2}");

        // 验证反序列化结果的正确性
        using var streamA = new MemoryStream(_colaAData);
        using var streamB = new MemoryStream(_colaBData);
        var packetA = _serializer.Deserialize<CoLaA>(streamA);
        var packetB = _serializer.Deserialize<CoLaB>(streamB);

        var deviceInfoA = packetA.Content.Payload.CommandConnent as UniqueIdentificationModeCommand;
        var deviceInfoB = packetB.Content.Payload.CommandConnent as UniqueIdentificationModeCommandB;

        Assert.Multiple(() =>
        {
            Assert.That(deviceInfoA.Name, Is.EqualTo("LMS10x_FieldEval"));
            Assert.That(deviceInfoA.Version, Is.EqualTo("V1.36-21.10.2010"));
            Assert.That(deviceInfoB.Name, Is.EqualTo("LMS10x_FieldEval"));
            Assert.That(deviceInfoB.Version, Is.EqualTo("V1.36-21.10.2010"));
        });
    }
} 