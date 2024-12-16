using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 用于处理扫描数据的命令模式类
    /// 该类包含扫描数据的相关信息，符合文档中的 sRA LMDscandata 响应结构
    /// 此类是只读的，用于解析从设备读取的扫描数据信息
    /// </summary>
    public class LMDscandataModeCommand : CommandBase
    {
        /// <summary>
        /// 用于检测版本的格式更改
        /// 到目前为止，版本始终为 0 或 1
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Versionnumber { get; set; }

        /// <summary>
        /// 用 SOPAS 定义
        /// </summary>
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string DeviceNumber { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        [FieldOrder(3)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public DeviceStatusEnum DeviceStatus { get; set; }

        /// <summary>
        /// 在扫描仪中完成并传输到接口的测量电报数量
        /// </summary>
        [FieldOrder(4)]
        public SickValue<ushort> TelegramCounter { get; set; }

        /// <summary>
        /// 扫描计数器
        /// </summary>
        [FieldOrder(5)]
        public SickValue<ushort> ScanCounter { get; set; }

        /// <summary>
        /// 自启动以来的时间，单位为 μs
        /// </summary>
        [FieldOrder(6)]
        public SickValue<uint> TimeSinceStartup { get; set; }

        /// <summary>
        /// 将完整扫描传输到缓冲区以进行数据输出的时间，单位为 μs；在扫描仪启动时从 0 开始
        /// </summary>
        [FieldOrder(7)]
        public SickValue<uint> TimeOfTransmission { get; set; }

        /// <summary>
        /// 数字输入状态
        /// </summary>
        [FieldOrder(8)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public InputStatusEnum InputStatus { get; set; }

        /// <summary>
        /// 数字输入状态 2
        /// </summary>
        [FieldOrder(9)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string InputStatus2 { get; set; }

        /// <summary>
        /// 数字输出状态
        /// </summary>
        [FieldOrder(10)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public OutputStatusEnum OutputStatus { get; set; }

        /// <summary>
        /// 数字输出状态 2
        /// </summary>
        [FieldOrder(11)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string OutputStatus2 { get; set; }

        /// <summary>
        /// 预留字段
        /// </summary>
        [FieldOrder(12)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Reserved { get; set; }

        /// <summary>
        /// 扫描频率
        /// </summary>
        [FieldOrder(13)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string ScanFrequency { get; set; }

        /// <summary>
        /// 测量频率
        /// </summary>
        [FieldOrder(14)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string MeasurementFrequency { get; set; }

        /// <summary>
        /// 编码器数量
        /// </summary>
        [FieldOrder(15)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string EncoderNumber { get; set; }

        /// <summary>
        /// 编码器列表
        /// </summary>
        [FieldOrder(16)]
        [ItemLength(0)] // TODO 未实现编码器解析
        [FieldLength(0)]
        public List<RadarEncoder> EncoderList { get; set; }

        /// <summary>
        /// 16 位通道数量
        /// </summary>
        [FieldOrder(17)]
        public SickValue<ushort> Channel16BitNumber { get; set; }

        /// <summary>
        /// 16 位通道列表
        /// </summary>
        [FieldOrder(18)]
        [FieldCount(nameof(Channel16BitNumber) + "." + nameof(SickValue<ushort>.Value))]
        public List<OutputChannel> OutputChannelList { get; set; }

        /// <summary>
        /// 8 位通道数量
        /// </summary>
        [FieldOrder(19)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> Channel8BitNumber { get; set; }

        /// <summary>
        /// 8 位通道列表
        /// </summary>
        [FieldOrder(20)]
        [FieldCount(nameof(Channel8BitNumber) + "." + nameof(SickValue<ushort>.Value))]
        public List<OutputChannel> OutputChannel8BitList { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [FieldOrder(21)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Position { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        [FieldOrder(22)]
        public FixedLengthString DeviceName { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        [FieldOrder(23)]
        public FixedLengthString Comment { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        [FieldOrder(24)]
        public TimeStruct Time { get; set; }

        /// <summary>
        /// 事件信息
        /// </summary>
        [FieldOrder(25)]
        public EventInfo EventInfo { get; set; }
    }
    /// <summary>
    /// 表示数字输出状态的枚举
    /// </summary>
    public enum OutputStatusEnum
    {
        /// <summary>
        /// 所有输出低电平
        /// 长度为2位: "0 0"
        /// 适用于所有传感器
        /// </summary>
        [SerializeAsEnum("0 0")]
        AllOutputsLow = 0,

        /// <summary>
        /// 所有输出高电平
        /// 长度为2位: "F 0"
        /// 适用于 TIM5xx, TIM7xx, LDxxx
        /// </summary>
        [SerializeAsEnum("F 0")]
        AllOutputsHigh_TIM5xx_TIM7xx_LDxxx = 0xF0,

        /// <summary>
        /// 所有输出高电平
        /// 长度为2位: "2 0"
        /// 适用于 TIM24x
        /// </summary>
        [SerializeAsEnum("2 0")]
        AllOutputsHigh_TIM24x = 0x20,

        /// <summary>
        /// 所有内部输出高电平
        /// 长度为2位: "3F 0"
        /// 适用于 LMS1xx
        /// </summary>
        [SerializeAsEnum("3F 0")]
        AllInternalOutputsHigh_LMS1xx = 0x3F00,

        /// <summary>
        /// 所有输出高电平（包括外部输出）
        /// 长度为4位: "07 FF"
        /// 适用于 LMS1xx
        /// </summary>
        [SerializeAsEnum("07 FF")]
        AllOutputsHighIncludingExternal_LMS1xx = 0x07FF,

        /// <summary>
        /// 所有内部输出高电平
        /// 长度为2位: "3F 0"
        /// 适用于 LMS5xx
        /// </summary>
        [SerializeAsEnum("3F 0")]
        AllInternalOutputsHigh_LMS5xx = 0x3F00,

        /// <summary>
        /// 所有输出高电平（包括外部输出）
        /// 长度为4位: "3F FF"
        /// 适用于 LMS5xx
        /// </summary>
        [SerializeAsEnum("3F FF")]
        AllOutputsHighIncludingExternal_LMS5xx = 0x3FFF,

        /// <summary>
        /// 输出1高电平
        /// 长度为2位: "1 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("1 0")]
        Output1High_LMS4000 = 0x10,

        /// <summary>
        /// 输出2高电平
        /// 长度为2位: "2 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("2 0")]
        Output2High_LMS4000 = 0x20,

        /// <summary>
        /// 输出1和输出2都高电平
        /// 长度为2位: "3 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("3 0")]
        Output1And2High_LMS4000 = 0x30,

        /// <summary>
        /// 输出4高电平
        /// 长度为2位: "8 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("8 0")]
        Output4High_LMS4000 = 0x80,

        /// <summary>
        /// 输出1和输出4都高电平
        /// 长度为2位: "9 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("9 0")]
        Output1And4High_LMS4000 = 0x90,

        /// <summary>
        /// 输出2和输出4都高电平
        /// 长度为2位: "A 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("A 0")]
        Output2And4High_LMS4000 = 0xA0,

        /// <summary>
        /// 输出1、输出2和输出4都高电平
        /// 长度为2位: "B 0"
        /// 适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("B 0")]
        Output1And2And4High_LMS4000 = 0xB0,

    }
    /// <summary>
    /// 表示数字输入状态的枚举
    /// </summary>
    public enum InputStatusEnum
    {
        /// <summary>
        /// 所有输入低电平
        /// 长度为2位: "0 0"
        /// 适用于 LMS1xx, LMS5xx, LMS4000, TiM24x, MRS 1000, LMS1000, MRS6000, LRS4000
        /// </summary>
        [SerializeAsEnum("00")]
        AllLow = 0,

        /// <summary>
        /// 所有输入高电平
        /// 长度为2位: "3 0"
        /// 适用于 LMS1xx, LMS5xx, LMS4000
        /// </summary>
        [SerializeAsEnum("30")]
        AllHigh = 3,

        /// <summary>
        /// LMS4000 输入1高电平，输入2低电平
        /// 长度为2位: "1 0"
        /// 仅适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("10")]
        Input1HighLMS4000 = 1,

        /// <summary>
        /// LMS4000 输入1低电平，输入2高电平
        /// 长度为2位: "2 0"
        /// 仅适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("20")]
        Input2HighLMS4000 = 2,

        /// <summary>
        /// LMS4000 输入1和输入2都高电平
        /// 长度为2位: "3 0"
        /// 仅适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("30")]
        Input1And2HighLMS4000 = 3
    }
    /// <summary>
    /// 表示设备状态的枚举
    /// </summary>
    public enum DeviceStatusEnum
    {
        /// <summary>
        /// 所有设备正常运行
        /// 长度为2位: "0 0"
        /// </summary>
        [SerializeAsEnum("00")]
        AllOk = 0,

        /// <summary>
        /// 所有设备错误
        /// 长度为2位: "0 1"
        /// </summary>
        [SerializeAsEnum("01")]
        AllError = 1,

        /// <summary>
        /// LMS1xx 和 LMS5xx 污染警告
        /// 长度为2位: "0 2"
        /// 仅适用于 LMS1xx 和 LMS5xx
        /// </summary>
        [SerializeAsEnum("02")]
        PollutionWarningLMS1xxLMS5xx = 2,

        /// <summary>
        /// LMS1xx 和 LMS5xx 污染错误，无设备错误
        /// 长度为2位: "0 4"
        /// 仅适用于 LMS1xx 和 LMS5xx
        /// </summary>
        [SerializeAsEnum("04")]
        PollutionErrorNoDeviceErrorLMS1xxLMS5xx = 4,

        /// <summary>
        /// LMS1xx 和 LMS5xx 污染错误，伴有设备错误
        /// 长度为2位: "0 5"
        /// 仅适用于 LMS1xx 和 LMS5xx
        /// </summary>
        [SerializeAsEnum("05")]
        PollutionErrorWithDeviceErrorLMS1xxLMS5xx = 5,

        /// <summary>
        /// LMS4000 激光关闭时的数据输出
        /// 长度为2位: "1 0"
        /// 仅适用于 LMS4000
        /// </summary>
        [SerializeAsEnum("10")]
        DataOutputDuringLaserOffLMS4000 = 10
    }
}