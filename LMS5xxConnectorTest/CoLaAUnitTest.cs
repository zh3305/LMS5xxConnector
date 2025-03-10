﻿using System.Text;
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
                  //  string response = "\x02sRA LMDscandata 1 1 89A27F 0 0 343 347 27477BA9 2747813B 0 0 7 0 0 1388 168 0 1 DIST1 3F800000 00000000 186A0 1388 15 8A1 8A5 8AB 8AC 8A6 8AC 8B6 8C8 8C2 8C9 8CB 8C4 8E4 8E1 8EB 8E0 8F5 908 8FC 907 906 0 0 0 0 0 0\x03";
                    string response =
                        "\u0002sSN LMDscandata 0 1 154BD4C 0 0 D438 D43C 5ADF6924 5ADF97D9 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF3CB0 1A0B 11E 2052 2042 1ECC 1EBD 1EB7 1FFE 2491 249B 24AB 24CD 24DC 24BC 243D 2490 24E8 24D2 24D1 24C7 24C7 24C8 21FF 1F11 1F20 1F36 20BF 20D0 20EF 20FC 2118 212E 2147 2162 2182 2199 21C0 21DB 2203 2226 2250 2275 229B 22C7 2285 2191 20AD 1FD7 1660 1689 16B6 16E3 1673 1544 1560 1581 1BDC 1B2C 1AA7 1A36 19C0 1951 18E4 187A 181A 17B9 16A9 164B 15FD 15B1 1561 151B 1509 14FA ECB 1477 1479 F65 10DC 1094 FFF 1356 1331 EB9 E9C EA7 1258 1213 11DF 1174 1187 1157 1135 1116 10F3 10D1 10B4 1093 107D 1059 103E 101F 100F FF0 FD7 FC0 FA5 F8F F78 F6B F59 F43 F2D F24 F14 F05 EF2 EE2 ED4 EBF EB8 EAD E96 E90 E7E E74 E77 E6D E64 E5A E4F E46 E40 E3C E31 E2E E2D E25 E26 E21 E21 E19 E1C E18 E13 E16 E16 E1A E23 E1E E24 E1E E2A E29 E2E E35 E3A E35 E43 E4A E59 E5C E66 E70 E73 E7F E8B E8F E77 DD7 D49 CCD CB0 CC0 CCF CD7 CE6 CF1 D06 D15 D28 D34 D49 D5D FA8 F66 F17 EBF E7C E33 DED DA8 D66 D25 CEF CB7 C7A C48 C13 BDD BB9 B87 B5B B35 B01 AE3 AB9 A96 A6E A4E A32 A12 9F1 9DD 9BC 9A0 988 968 958 93C 92C 918 8F9 8E2 8CE 8BB 8AC 897 887 875 868 857 845 833 821 814 808 801 7F0 7DA 7D1 7E8 7C3 7B1 7AB 79D 79B 792 78F 790 770 75E 763 78B 7C7 7B5 79B 7C0 7DE 7FD 7FD 7FC 870 8ED 988 A36 AFF BCF C04 C0A BFC C33 328 320 316 313 325 347 2E5 2F2 2F0 2ED 2EC 2E5 2EF 2F6 2F3 2EA 0 0 0 0 0 0\u0003i٧#\tL\u0748\aUnknown\\\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D439 D43D 5ADF904C 5ADFBEF4 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF49B6 1A0B 11D 204D 2035 1EC3 1EBD 1ECB 2492 24A2 24AB 24BB 24C7 24D2 2498 243C 24DB 24DE 24CC 24CE 24C7 24C7 2464 201D 1F1A 1F28 1F56 20D0 20E7 20F1 210A 2122 2135 2158 2173 218F 21AB 21D1 21F1 2211 2239 225D 2289 22B5 22D9 21FD 211F 203E 1F6C 1676 16A9 16D3 16B0 1617 1546 1571 1596 1B6D 1AEF 1A71 19F7 1985 1913 18AE 1846 17DD 1761 1677 1625 15D9 158B 1541 14FE 1512 1509 14C2 145E F70 F85 10F0 1003 1009 DFB ECC EA1 EA6 EBC 1224 11EE 1157 119E 1177 114A 1123 10FD 10E1 10BD 10A8 108B 106A 104C 1030 101A FFF FE0 FD3 FB7 FA1 F8F F77 F67 F4E F3E F27 F14 F04 EFA EE4 ED3 EC5 EC4 EB4 EA1 E93 E88 E83 E76 E68 E65 E58 E58 E47 E3E E3E E35 E35 E30 E27 E25 E21 E25 E1C E1C E19 E1B E15 E15 E1A E13 E1A E23 E20 E27 E22 E2E E2F E36 E39 E40 E45 E52 E4F E5E E66 E68 E79 E84 E8D E98 E1E D8B D0E CAF CAF CC2 CD0 CE3 CF0 D00 D09 D1D D32 D3F D53 E5E F8A F3B EEA E96 E58 E08 DD0 D7E D4B D0B CDA C94 C5D C34 C01 BCB BA7 B6F B46 B19 AF3 ACD AAF A81 A5F A42 A21 A04 9EB 9CC 9AF 996 97A 964 94F 934 915 902 8F3 8E0 8C4 8B4 89F 891 87B 86F 85D 843 841 82B 81A 813 7FD 7F5 7EC 7DB 7D0 7E6 7B8 7AF 79A 79E 791 789 792 769 76A 75F 77F 794 7CB 7A4 7A3 7E1 7E5 7FE 7F8 833 8AC 93A 9DF A95 B63 C06 C09 C00 C22 346 328 317 30E 310 353 C27 2F0 2EE 2F0 2F2 2E1 2ED 2F5 2F5 2F3 0 0 0 0 0 0\u0003\u009a\u001ej#\tL\u0748\aUnknown]\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D43A D43E 5ADFB772 5ADFE61A 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF4333 1A0B 11D 204D 2046 1EC3 1EBA 1EBD 24CE 249E 24A8 24B4 24C6 24D7 24B2 2441 24B7 24DB 24D0 24D0 24CD 24C6 24C9 2083 1F15 1F25 1F34 20C7 20DA 20F1 2103 211B 2136 2154 216B 2187 21A7 21C4 21DD 220D 2232 2257 2281 22AA 22D2 2247 2154 2074 1FA2 166C 16AD 16BC 16E0 1659 1542 1568 158D 1B8C 1B0C 1A8B 1A1C 19A1 1936 18C6 1861 17F7 179E 1690 163C 15EB 159D 1552 150B 151C 1522 EE4 1464 147E F6A 1108 1021 FFE 133E 1319 EA4 E9E EAD 123A 1200 119A 11A2 117C 1156 112E 110B 10EA 10CD 10AB 108B 1073 1056 103E 1017 FFC FE6 FCE FBF FA3 F8B F7D F66 F56 F3F F2F F1E F0D EF9 EF4 EE0 ECC EC5 EB3 EAA E90 E8B E7A E7F E71 E5D E5C E56 E4D E48 E44 E3C E34 E35 E29 E2A E21 E20 E1E E16 E19 E20 E12 E10 E1D E1D E2A E1B E20 E25 E24 E29 E35 E30 E39 E46 E47 E52 E51 E5D E6C E65 E77 E7C E88 E92 E3D DAF D33 CAD CB1 CBD CD0 CD7 CE9 CFE D07 D1B D2D D41 D50 D6B F9B F50 EFD EB1 E65 E22 DD7 D92 D58 D21 CDD CAC C6D C3E C04 BD9 BAA B75 B4F B28 AF7 ADA AB2 A95 A67 A49 A2E A10 9F2 9D1 9B3 99E 989 967 951 934 91C 910 8F5 8DF 8CF 8B6 8A3 890 882 876 85C 84C 840 834 820 810 806 7F5 7E9 7DC 7CE 7F4 7BB 7B4 7A6 79C 795 787 789 786 76F 761 772 787 7CD 7AB 79F 7D1 7E6 7FE 7F4 810 890 90D 9B2 A66 B2F BED C05 C08 C01 338 329 317 30F 30B 33B C17 2F2 2ED 2F5 2E3 2EA 2E8 2ED 2F8 2ED 0 0 0 0 0 0\u0003ʨk#\tL\u0748\aUnknown\\\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D43B D43F 5ADFDE92 5AE00E41 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF5039 1A0B 11D 2047 1F07 1EBA 1EBC 1EE6 2488 249C 24A4 24B6 24D1 24E0 243B 246F 24E0 24D7 24D5 24C7 24C1 24C5 22BA 1F40 1F1D 1F34 1FCB 20D6 20EA 20FE 2113 2121 213F 215F 2175 2195 21B0 21DD 21F3 2221 2246 226C 2294 22BA 22C7 21CD 20E7 200A 1F8D 167B 16B3 16D5 1691 1556 1553 1575 1BE8 1B4A 1ACA 1A4F 19DB 196A 18FD 1892 1827 17CD 16D1 1669 160F 15BF 1578 1532 1504 1511 ED3 14A5 1457 F67 106A 10D6 FFC 1348 1324 EC2 EA9 E9F EE8 1215 11E8 1149 1196 116C 113A 111D 10F8 10DE 10BA 1099 107E 105F 1046 1023 100E FFA FDF FC9 FAB F97 F89 F69 F5B F48 F39 F29 F0B F07 EF3 EE4 ED6 ECB EB8 EAE EA2 E97 E85 E80 E76 E6E E64 E5A E4F E4D E40 E3C E34 E2F E2D E26 E27 E23 E1D E20 E1D E1C E14 E1B E18 E1F E17 E23 E1E E1D E27 E26 E30 E36 E35 E3B E3D E48 E4A E5B E62 E6B E75 E80 E88 E94 E92 DFA D6A CED CA8 CB4 CC8 CDA CE1 CF4 CFF D16 D27 D40 D48 D5A F99 F7E F2C EDC E8E E41 DFE DBC D7F D37 CFD CC4 C8A C56 C29 BED BBF B8F B6A B42 B0C AE3 AC9 A9D A77 A5F A3C A20 9F8 9DF 9C8 9A9 992 979 95B 947 931 918 900 8E8 8D1 8C1 8B3 8A1 889 879 867 855 84C 835 826 81C 809 7FC 7F0 7E9 7D4 7DB 7DF 7BA 7B2 7A0 794 78B 787 79A 76C 76A 75F 78A 7B1 7C3 79B 7AB 7E6 7EB 801 7F9 859 8CC 95E A08 AC9 B9C C0F C03 C02 C2E 334 331 315 313 319 387 2E7 2ED 2E9 2F0 2E7 2EB 2EC 2F2 2F5 2F0 0 0 0 0 0 0\u0003'\u001cn#\tL\u0748\aUnknownc\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D43C D440 5AE005B1 5AE03441 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF3CB0 1A0B 11E 2055 2046 1ECC 1EBE 1EBC 1FFC 2496 24AA 24B0 24B8 24D1 24C4 2437 2493 24DC 24D3 24D1 24C6 24C4 24C5 2205 1F12 1F1C 1F34 20C6 20D9 20E5 2100 2117 2131 2148 2166 2180 219B 21BA 21E4 21FB 2224 224A 2272 229A 22CB 2287 2199 20AA 1FD8 1669 1681 16BD 16DA 1675 1547 155D 157E 1BDD 1B2C 1AA9 1A35 19BF 194C 18E0 187C 1810 17B1 16A5 1651 15FC 15B0 1562 1518 150D 14F2 ED5 147C 147D F64 10E0 1093 1004 1354 1331 EAC EA1 EB0 125B 1205 11DC 116D 118B 1164 1136 110D 10F3 10D3 10B0 1095 107C 1058 103B 1020 1007 FED FD5 FC3 FA3 F8E F7D F6A F53 F4C F2F F23 F13 EFB EE9 EE3 ED6 EC1 EB8 EAD E9D E91 E89 E7F E6F E63 E5C E58 E4C E4B E3F E38 E38 E31 E21 E24 E24 E1A E1C E1C E14 E19 E18 E1E E16 E1E E1C E23 E1D E26 E27 E2A E2C E33 E3A E3B E42 E4A E55 E57 E5E E6B E70 E7F E8B E91 E79 DD1 D4F CD4 CAE CB7 CCE CD6 CE4 CF6 D05 D10 D28 D39 D4A D5D FA4 F66 F11 EC8 E74 E34 DF2 DA9 D64 D33 CF2 CB7 C7F C49 C10 BE5 BB2 B83 B5D B37 B08 ADF AB8 A97 A75 A53 A2C A17 9F0 9D4 9BB 99C 989 970 957 93C 928 90C 901 8EA 8D3 8B7 8AD 896 881 872 861 857 83F 835 82A 813 806 7F9 7EC 7E5 7D3 7EA 7C1 7B3 7A8 79F 79A 78C 77A 792 771 765 774 786 7C0 7B6 7A0 7BE 7E0 800 7F5 800 86C 8F0 97C A3B AFE BCB C03 C00 C05 C32 328 326 317 30E 324 365 2EC 2EB 2EA 2EE 2E8 2E9 2F0 2F2 2EF 2F3 0 0 0 0 0 0\u0003\u0011\u00b2o#\tL\u0748\aUnknown\\\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D43D D441 5AE02CCC 5AE05B57 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF49B6 1A0B 11D 204C 2035 1EC0 1EB8 1EC1 2492 2495 24AC 24BC 24C6 24E0 24A3 243E 24D3 24D8 24CE 24CD 24C5 24C8 248F 2036 1F1A 1F29 1F4F 20C7 20E0 20F9 2111 211F 2144 2156 2179 2191 21B5 21CE 21F1 2210 223B 225A 2285 22B3 22DA 2206 2120 2046 1F67 1675 16AE 16C9 16B0 1631 1550 156E 158F 1B6A 1AEE 1A6E 19FC 1987 1918 18AC 1844 17E8 1763 167C 1627 15D6 1586 153B 14FC 1518 151C 14BF 1460 F6A F7F 10EF 1011 100A E02 EC8 EA2 EA3 EBB 121E 11EF 1158 11A0 1174 114B 112B 1103 10E3 10BE 10A5 108C 106A 104B 102B 1015 FFC FE8 FCC FB8 F9E F89 F74 F62 F4A F36 F2B F15 F07 EF8 EE9 ED1 ECE EBE EB0 EA8 E99 E91 E82 E6C E6A E67 E60 E52 E4F E44 E3A E33 E37 E2C E30 E27 E24 E21 E1D E1C E25 E13 E1A E19 E19 E1A E21 E1C E22 E23 E28 E2B E2D E33 E36 E42 E53 E4F E5A E61 E66 E6A E73 E8A E8B E96 E23 D8B D0E CAD CB2 CC3 CD3 CE0 CED CF8 D0B D1C D2C D3F D56 DF3 F8C F40 EE9 EA3 E5A E08 DC3 D89 D4E D10 CCB C96 C66 C34 BF7 BC7 B99 B6C B4A B1A AFA ACD AA9 A86 A68 A45 A23 A00 9E7 9C7 9B5 997 97B 964 948 936 918 906 8E8 8D6 8C4 8B1 89C 890 880 869 85C 848 83D 82C 81D 80F 7FE 7EE 7E4 7D5 7D4 7E7 7B9 7B0 7A0 79B 78E 789 796 76F 763 763 787 791 7CA 7A2 79F 7E2 7E7 7FC 7F6 836 8AC 939 9D7 A97 B63 C0C C05 C04 C1B 339 325 31A 317 316 35A C21 2EB 2EF 2ED 2ED 2E0 2ED 2EB 2F1 2F1 0 0 0 0 0 0\u0003\u0097\u0010r#\tL\u0748\aUnknown_\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D43E D442 5AE053E2 5AE08279 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF4333 1A0B 11D 2050 2048 1EBE 1EBF 1EBB 24CC 249E 24A5 24AF 24C3 24DB 24B7 243A 24BD 24DB 24D9 24CA 24C9 24C7 24C9 207E 1F1A 1F2B 1F33 20CE 20DB 20EF 2109 211C 2139 2159 2169 218C 21A1 21C1 21ED 2210 2238 225B 2280 22A3 22D3 2241 2154 207A 1FA4 1672 16B1 16C5 16D2 165A 153A 1563 1587 1B8E 1B08 1A86 1A1A 19A4 1933 18CD 1855 17F9 17A1 168B 163E 15E2 1599 154E 1509 151A 1521 EE2 146C 1488 F5F 1110 101A 1008 1337 131D EA7 EA5 EBB 123B 1200 1196 11A3 117B 1154 1130 110A 10EC 10CC 10B0 108D 106D 104F 102F 1021 1003 FE8 FD0 FBA F9E F90 F7D F6A F57 F3D F2D F1A F09 EF9 EE8 ED9 ECF EBE EB4 EA2 E9D E92 E7F E77 E67 E6F E5D E54 E49 E4A E35 E39 E2C E2D E2A E1C E23 E1C E1F E19 E18 E1C E15 E17 E1B E1F E1B E25 E23 E24 E29 E30 E35 E32 E33 E3F E46 E48 E52 E57 E65 E70 E71 E7F E8A E91 E47 DB5 D2D CA5 CAF CBF CD0 CDC CEC CFA D06 D18 D2A D44 D51 D71 F98 F4F F02 EB0 E66 E24 DD4 D9A D62 D19 CDD C9F C6B C34 C09 BD8 BAF B78 B50 B26 B07 AD1 AB5 A92 A69 A4B A26 A08 9F4 9D5 9B5 9A0 984 965 94F 934 91E 90E 8F4 8E2 8CD 8B3 8A5 88D 886 86C 85E 854 840 829 827 811 808 7FC 7EC 7DC 7CF 7F5 7BE 7AA 7A1 79F 794 783 786 787 76C 763 771 78A 7C5 7B0 79F 7D7 7E0 7FE 7F7 80D 890 911 9B2 A65 B2D BF4 C03 C06 C04 C35 32D 31D 312 30E 338 C26 2EE 2ED 2F2 2F4 2EE 2E4 2F7 2F3 2F2 0 0 0 0 0 0\u0003*¿s#\tL\u0748\aUnknown\\\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D43F D443 5AE07B06 5AE0AA76 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF5039 1A0B 11D 2050 1F09 1EC0 1EC0 1EEC 2498 24A0 24AD 24BC 24C8 24D9 243E 246C 24D8 24DA 24C9 24C9 24C5 24C5 22BD 1F3C 1F1A 1F32 1FC8 20D8 20EA 20FD 210A 2130 213B 2163 217F 2197 21B8 21D5 21F8 2220 2244 226E 2291 22B7 22C9 21BF 20E1 200B 1F91 1680 16B2 16D5 168E 1555 1550 157C 1BF2 1B50 1AD1 1A54 19DA 196C 18FF 1897 182E 17D2 16D6 166C 1613 15C4 1574 152F 1504 150A ECD 14A8 144C F6D 106F 10D8 FFF 133D 131A EC1 E9C E9D EE0 1214 11E5 1145 1196 116E 1142 1118 10F3 10D6 10BF 1098 1084 1060 1045 102D 100B FF6 FE2 FC9 FAF F9F F86 F70 F57 F4A F3B F23 F14 EFF EF1 EE5 ED6 EBE EBE EA9 EA5 E94 E89 E7B E74 E68 E5F E5D E57 E42 E41 E38 E31 E29 E2A E27 E2B E1A E1A E1B E17 E1B E12 E16 E1A E19 E1A E1B E24 E27 E28 E2A E31 E37 E31 E36 E43 E47 E4B E58 E64 E68 E72 E77 E81 E8F E9C DFB D6B CF3 CAE CB6 CC8 CD5 CE2 CEF D01 D0A D23 D31 D43 D56 F9E F7A F27 ED8 E88 E4A DED DBD D79 D39 D08 CC5 C88 C5C C20 BEF BBE B93 B69 B3A B10 AE8 AC0 AA1 A80 A55 A3B A1C 9FF 9DD 9BF 9A5 98E 976 957 943 92B 917 8FA 8E9 8D3 8BE 8B2 895 890 87E 869 856 844 840 82D 814 80C 7FE 7F1 7EA 7DA 7E2 7DE 7B6 7A9 79C 79E 78E 78C 7A0 76F 767 765 789 7B3 7BF 7A5 7AD 7E2 7EB 800 7F8 852 8D6 95A A0E ACA B93 C09 C03 BFF C2E 330 32A 313 311 313 382 2F1 2E7 2EF 2EE 2EB 2E7 2EE 2F1 2F8 2ED 0 0 0 0 0 0\u0003µ\u008cu#\tL\u0748\aUnknownb\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D440 D444 5AE0A22A 5AE0D0E8 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF3CB0 1A0B 11E 2052 2046 1ECA 1EBD 1EBD 1FFD 2492 24A5 24B1 24C1 24CC 24B7 2432 248E 24DB 24DC 24CD 24C9 24C2 24CD 2200 1F14 1F21 1F34 20BC 20DB 20ED 2105 2113 212E 214D 215E 2183 21A2 21BB 21DF 2202 2223 2245 2272 229E 22CC 2285 2187 20A5 1FD1 1663 1683 16B5 16DA 1679 153E 155D 1583 1BD4 1B2B 1AAE 1A34 19C3 1952 18E2 187B 1812 17BD 16A3 1652 15FD 15AF 1565 151A 1513 14F9 ED3 1476 147B F66 10DE 109B FFE 135A 132D EAF E9C EB4 125C 120A 11DF 116D 1188 115E 1138 1111 10F6 10D5 10B2 1097 107A 1055 104C 1023 100F FF2 FDA FC1 FAD F94 F83 F66 F61 F40 F2E F22 F0F EFC EF2 EDB ED4 EC6 EB7 EAD E9F E8C E84 E6D E71 E6A E5D E59 E4D E43 E3E E3A E35 E37 E2B E28 E21 E1F E1C E1B E18 E15 E1F E1A E16 E12 E21 E1B E1E E20 E20 E2B E29 E30 E37 E39 E3E E45 E4C E61 E5C E6C E70 E80 E84 E91 E75 DD5 D4A CC8 CAA CB8 CC8 CD3 CE4 CED D08 D12 D23 D36 D47 D5A F9E F60 F0F EC0 E7F E31 DE7 DAD D63 D27 CF0 CBA C7A C45 C10 BEA BB4 B86 B58 B28 B04 ADE ABF A92 A73 A4F A38 A12 9F3 9D6 9B6 9A0 98B 96E 95C 942 921 913 8FE 8E3 8CE 8BF 8AE 898 884 879 865 853 843 83A 81C 80F 80B 7F4 7E6 7E0 7D2 7E7 7C8 7B3 7A9 79B 795 78C 788 78E 76D 766 76D 789 7C7 7BC 7A5 7BC 7E7 7F7 7FE 7FA 86D 8F4 98B A2D AFC BD0 C0E BFF C04 C32 327 324 311 30F 31C 33D 2F1 2E6 2F3 2EA 2EB 2EB 2EC 2F2 2FD 2EC 0 0 0 0 0 0\u0003>{x#\tL\u0748\aUnknown\\\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D441 D445 5AE0C950 5AE0F7E5 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF49B6 1A0B 11D 2050 2033 1EBD 1EC0 1EC2 2495 249D 24AD 24B4 24CC 24DD 2499 2435 24DE 24DB 24D5 24CC 24C6 24BF 2459 2019 1F19 1F27 1F5C 20CA 20DF 20F8 210D 2121 2140 215A 216F 218D 21AA 21D1 21F7 2216 223B 2262 2286 22B1 22E0 2201 211E 203F 1F67 1679 16A3 16D3 16B4 1613 154A 156F 1594 1B6E 1AEB 1A73 19F9 1989 1916 18AC 1841 17E2 1756 1678 1623 15CF 158C 1543 1509 1516 1517 14C0 145C F65 F85 10F9 100A 1006 E03 EC8 EA3 E9E EC3 121C 11F3 1159 11A1 117D 1147 1127 1106 10E3 10C3 10A6 1085 106B 104A 102D 1018 FFD FE1 FD2 FB6 FA1 F8C F77 F5B F4B F34 F28 F1B F08 EF3 EEA EDC ECF EBB EAE EA6 E94 E86 E7E E73 E69 E6A E5F E5E E48 E43 E3D E3A E38 E2A E20 E22 E1F E17 E16 E17 E1A E18 E1D E20 E1D E17 E1D E1F E24 E23 E24 E28 E2C E35 E3C E3D E49 E4F E5B E59 E62 E6F E76 E80 E8F E9A E1B D83 D07 CAB CB7 CC3 CCF CE0 CE8 CFA D10 D22 D2C D3F D55 E33 F86 F41 EE9 E98 E5C E0F DCE D87 D4D D10 CCB C98 C5E C31 BF4 BCB BA4 B71 B43 B1C AF2 AD5 AA8 A85 A62 A45 A1F A06 9EC 9C8 9B2 997 97E 95B 940 929 91A 908 8F0 8DC 8C4 8B9 89F 88F 881 872 862 847 83C 823 81D 80F 7F7 7F4 7E6 7D9 7D9 7E7 7BB 7A7 79E 79E 792 78D 790 771 768 763 782 792 7CC 7A2 7AA 7E3 7E5 7FA 7F7 831 8AE 934 9DB A98 B62 C08 C0A C03 C21 341 32E 31A 313 30D 354 C26 2EA 2F4 2EE 2E8 2ED 2E9 2F2 2F1 2EE 0 0 0 0 0 0\u0003k\u001cz#\tL\u0748\aUnknown_\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D442 D446 5AE0F077 5AE11F11 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF4333 1A0B 11D 2052 204A 1EBC 1EBE 1EB5 24DF 2499 249F 24AC 24C2 24DF 24B0 243D 24BD 24D8 24D0 24CD 24C6 24C2 24C6 207F 1F1C 1F23 1F37 20CA 20DE 20F2 2104 2120 2136 2154 216D 218A 21A1 21C9 21EA 2207 223C 225B 227E 22AC 22D4 2247 2160 2071 1F9D 166E 16B7 16C8 16DA 1662 1541 1561 1589 1B91 1B06 1A90 1A18 19A3 1932 18C8 185D 17FB 179E 168F 1637 15EC 159E 154E 1503 1518 1513 EDA 146E 148F F6C 10FE 101E 1006 1331 1322 EA7 EA4 EAC 1235 11FD 119E 11A4 1183 115C 112D 110E 10EB 10C8 10B2 1093 1071 104F 1032 101D 1002 FE5 FD7 FBA FA2 F94 F79 F65 F51 F38 F2E F1D F07 F01 EEB ED7 ECA EBB EB0 EAD E94 E8F E86 E7C E6E E69 E61 E56 E47 E48 E42 E38 E2E E2F E2C E24 E23 E25 E19 E1F E12 E16 E1B E19 E1B E20 E1C E1D E21 E27 E23 E2F E2B E34 E38 E3B E43 E4B E51 E59 E65 E6C E74 E80 E91 E96 E40 DB2 D2A CA9 CB7 CBA CCF CDC CE6 CF4 D09 D1C D2D D38 D4F D74 F95 F51 F00 EAE E64 E1F DDE D98 D52 D1A CDE CA1 C70 C33 C06 BDA BA8 B85 B4F B26 B01 AD2 ABB A8F A68 A49 A2B A0D 9F1 9D2 9B2 99B 983 968 954 935 916 90B 8F1 8E0 8CB 8B9 8A3 88F 884 874 861 850 83A 830 823 80B 809 7F6 7E9 7D7 7CC 7F3 7BD 7AD 7A1 798 794 787 782 77F 773 763 775 787 7CA 7B1 7A1 7D7 7E0 801 7F6 80D 88A 915 9B1 A62 B2A BFB C04 C07 C06 338 327 321 310 313 33B C46 2E9 2E5 2F2 2EF 2E6 2EC 2F2 2F2 2EF 0 0 0 0 0 0\u0003ˢ|#\tL\u0748\aUnknown]\u0005  \u0002sSN LMDscandata 0 1 154BD4C 0 0 D443 D447 5AE1179C 5AE1470D 0 0 3F 0 0 2710 21C 0 1 DIST1 3F800000 00000000 FFFF5039 1A0B 11D 2049 1F0D 1EC0 1EBB 1EF1 2493 249C 24AA 24BD 24CE 24E3 243E 246F 24DD 24D7 24D0 24C9 24C6 24C2 22C2 1F47 1F21 1F30 1FCF 20D4 20E9 20FA 210D 212E 2144 2161 217E 219A 21B9 21DD 21FC 2223 2247 226C 2297 22BA 22C6 21CA 20EC 200D 1F85 167B 16B4 16CF 169E 155C 1551 1579 1BE4 1B4C 1ACA 1A54 19DC 1971 18FF 188D 182C 17D1 16DA 1666 1614 15C2 1570 1531 14FF 150A ED5 14A9 145A F6C 1078 10D3 1005 1334 1320 EBB E97 EAC EE4 1214 11E2 1149 118F 116A 1141 1124 10FD 10D5 10BB 109B 1084 1067 1045 102B 1010 FF9 FDD FC6 FB1 FA0 F7F F74 F5F F49 F33 F27 F0F F09 EEE EED ED2 EC8 EB2 EAE E9E E95 E85 E7E E71 E68 E57 E5A E4B E4B E43 E42 E31 E35 E28 E26 E27 E24 E18 E1F E18 E16 E1C E18 E1B E17 E1B E27 E23 E23 E2A E2C E2A E2D E33 E38 E40 E4D E51 E59 E62 E66 E75 E7E E88 E8D E9C DF7 D73 CE8 CAC CB9 CC5 CD4 CDD CF8 D05 D14 D28 D32 D49 D52 FA1 F7C F28 ED8 E8E E41 DFF DBB D7A D32 CFE CBE C8C C51 C25 BF5 BBA B8E B68 B32 B18 AE6 AC9 A9E A7F A57 A3C A1C A02 9DF 9CE 9A3 98D 975 959 946 92B 913 903 8E6 8D4 8C0 8B0 89D 88B 87E 869 854 848 836 82A 81D 805 7FF 7F2 7E4 7DB 7E0 7DD 7BD 7B5 79D 797 78F 786 79F 771 767 763 785 7B4 7C5 7A0 7B4 7E5 7E8 7F6 7F4 851 8D3 95B A0D AC3 B94 C0A C09 C02 C2D 32F 32A 319 313 316 36E 2E6 2F1 2EE 2F5 2ED 2E3 2E9 2FA 2F5 2F0 0 0 0 0 0 0\u0003";
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