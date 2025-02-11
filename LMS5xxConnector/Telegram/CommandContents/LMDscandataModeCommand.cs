using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    using LMS5xxConnector.Enums;
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
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
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
        public string InputStatus { get; set; }

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
        public string OutputStatus { get; set; }

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
}