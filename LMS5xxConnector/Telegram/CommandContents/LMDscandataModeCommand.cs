using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LMDscandataModeCommand : CommandBase
    {
        //用于检测版本的格式更改。到目前为止，版本始终为 0 或 1
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Versionnumber { get; set; }

        //用 SOPAS 定义
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string DeviceNumber { get; set; }

        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string SerialNumber { get; set; }

        [FieldOrder(3)] public DeviceStatus DeviceStatus { get; set; }

        //Number of measurement telegrams finished in the scanner and given to the interface. 3)
        [FieldOrder(4)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> TelegramCounter { get; set; }

        //Scan counter
        [FieldOrder(5)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> ScanCounter { get; set; }

        // Time since start up in μs
        [FieldOrder(6)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<uint> TimeSinceStartup { get; set; }

        // Time of transmis- sion in μs
        //将完整的扫描传输到缓冲区以进行数据输出的时间，单位为 μs；在扫描仪启动时从 0 开始
        [FieldOrder(7)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<uint> TimeOfTransmission { get; set; }

        // Status of digital in-puts
        //All inputs low: 0 0
        // All inputs high: 3 0
        [FieldOrder(8)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string InputStatus { get; set; }

        [FieldOrder(9)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string InputStatus2 { get; set; }

        //Status of digital outputs
        // All internal outputs high:3F 0
        // All outputs high (inkl. Ext.Out): 3F FF
        [FieldOrder(10)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string OutputStatus { get; set; }

        [FieldOrder(11)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string OutputStatus2 { get; set; }

        //Reserved
        [FieldOrder(12)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Reserved { get; set; }

        //Scan fre-quency
        //25 Hz: +2500d (9C4h)
        // 35 Hz: +3500d (DACh)
        // 50 Hz: +5000d (1388h)
        // 75 Hz: +7500d (1A0Bh)
        // 100 Hz: +10000d(2710h)
        [FieldOrder(13)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string ScanFrequency { get; set; }

        //测量频率
        //Measurement frequency
        [FieldOrder(14)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string MeasurementFrequency { get; set; }

        //编码器数量
        //Number of encoders
        [FieldOrder(15)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string EncoderNumber { get; set; }

        // 编码器列表
        [FieldOrder(16)]
        [ItemLength(0)] //TODO 未实现编码器解析
        [FieldLength(0)]
        public List<RadarEncoder> EncoderList { get; set; }

        //Number of 16-bit channels
        [FieldOrder(17)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> Channel16BitNumber { get; set; }


        [FieldOrder(18)]
        [FieldCount(nameof(Channel16BitNumber) + "." + nameof(SickValue<ushort>.Value))]
        public List<OutputChannel> OutputChannelList { get; set; }


        // Amount of  8 bit channels
        [FieldOrder(19)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> Channel8BitNumber { get; set; }

        // 8 bit channel list
        [FieldOrder(20)]
        [FieldCount(nameof(Channel8BitNumber) + "." + nameof(SickValue<ushort>.Value))]
        public List<OutputChannel> OutputChannel8BitList { get; set; }

        //Position
        [FieldOrder(21)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Position { get; set; }

        //Device name
        [FieldOrder(22)] public FixedLengthString DeviceName { get; set; }

        //Comment
        [FieldOrder(23)] public FixedLengthString Comment { get; set; }

        //Time
        [FieldOrder(24)] public TimeStruct Time { get; set; }

        //Event info
        [FieldOrder(25)] public EventInfo EventInfo { get; set; }
    }
}