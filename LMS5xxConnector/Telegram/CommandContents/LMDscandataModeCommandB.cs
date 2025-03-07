using BinarySerialization;
using LMS5xxConnector.Enums;
using System;
using System.Collections.Generic;

namespace LMS5xxConnector.Telegram.CommandContents
{
    /// <summary>
    /// 表示CoLa B协议中的扫描数据命令模式类。
    /// 该类包含扫描数据的相关信息，符合文档中的sRA LMDscandata/sSN LMDscandata响应结构。
    /// </summary>
    public class LMDscandataModeCommandB : CommandBaseB
    {
        /// <summary>
        /// 获取或设置版本号，用于检测格式更改。
        /// 到目前为止，版本始终为0或1。
        /// </summary>
        [FieldOrder(0)]
        public ushort VersionNumber { get; set; }

        /// <summary>
        /// 获取或设置设备号。
        /// </summary>
        [FieldOrder(1)]
        public ushort DeviceNumber { get; set; }

        /// <summary>
        /// 获取或设置序列号。
        /// </summary>
        [FieldOrder(2)]
        public uint SerialNumber { get; set; }

        /// <summary>
        /// 获取或设置设备状态。
        /// 0 0: OK
        /// 0 1: Error
        /// </summary>
        [FieldOrder(3)]
        [FieldLength(2)]
        public byte[] DeviceStatus { get; set; } = new byte[2];

        /// <summary>
        /// 获取或设置电报计数器。
        /// 计数在扫描仪中完成并传递到接口的测量电报数量。
        /// </summary>
        [FieldOrder(4)]
        public ushort TelegramCounter { get; set; }

        /// <summary>
        /// 获取或设置扫描计数器。
        /// 计数设备中创建的扫描数量；计算实际完成的扫描次数。
        /// </summary>
        [FieldOrder(5)]
        public ushort ScanCounter { get; set; }

        /// <summary>
        /// 获取或设置自启动以来的时间（微秒）。
        /// 从设备通电开始计时；从0开始。
        /// </summary>
        [FieldOrder(6)]
        public uint TimeSinceStartup { get; set; }

        /// <summary>
        /// 获取或设置传输时间（微秒）。
        /// 完整扫描传输到数据输出缓冲区的时间（微秒）；从扫描仪启动时的0开始。
        /// </summary>
        [FieldOrder(7)]
        public uint TimeOfTransmission { get; set; }

        /// <summary>
        /// 获取或设置数字输入状态。
        /// 低字节表示输入1。
        /// </summary>
        [FieldOrder(8)]
        [FieldLength(2)]
        public byte[] StatusOfDigitalInputs { get; set; } = new byte[2];

        /// <summary>
        /// 获取或设置数字输出状态。
        /// 低字节表示输出1。
        /// </summary>
        [FieldOrder(9)]
        [FieldLength(2)]
        public byte[] StatusOfDigitalOutputs { get; set; } = new byte[2];

        /// <summary>
        /// 获取或设置层角度。
        /// 以前是保留字段，现在是层角度。
        /// </summary>
        [FieldOrder(10)]
        public ushort LayerAngle { get; set; }

        /// <summary>
        /// 获取或设置扫描频率。
        /// 单位：1/100 Hz
        /// </summary>
        [FieldOrder(11)]
        public uint ScanFrequency { get; set; }

        /// <summary>
        /// 获取或设置测量频率。
        /// 两次测量射击之间的时间的倒数（以100 Hz为单位）。
        /// </summary>
        [FieldOrder(12)]
        public uint MeasurementFrequency { get; set; }

        /// <summary>
        /// 获取或设置编码器数量。
        /// 如果为0，则接下来的两个值将缺失。
        /// </summary>
        [FieldOrder(13)]
        public ushort AmountOfEncoder { get; set; }

        /// <summary>
        /// 获取或设置编码器位置。
        /// 仅当AmountOfEncoder > 0时存在。
        /// </summary>
        [FieldOrder(14)]
        [SerializeWhen(nameof(AmountOfEncoder), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public uint? EncoderPosition { get; set; }

        /// <summary>
        /// 获取或设置编码器速度。
        /// 仅当AmountOfEncoder > 0时存在。
        /// </summary>
        [FieldOrder(15)]
        [SerializeWhen(nameof(AmountOfEncoder), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public ushort? EncoderSpeed { get; set; }

        /// <summary>
        /// 获取或设置16位通道数量。
        /// 提供测量数据的16位通道数。
        /// </summary>
        [FieldOrder(16)]
        public ushort AmountOf16BitChannels { get; set; }

        /// <summary>
        /// 获取或设置16位输出通道内容。
        /// 定义输出通道的内容，如DIST1、RSSI1等。
        /// </summary>
        [FieldOrder(17)]
        [FieldCount(nameof(AmountOf16BitChannels))]
        public string[] OutputChannel16Bit { get; set; }

        /// <summary>
        /// 获取或设置比例因子。
        /// 测量值的比例因子（对于LMS5xx，这取决于角分辨率）。
        /// </summary>
        [FieldOrder(18)]
        [FieldCount(nameof(AmountOf16BitChannels))]
        public float[] ScaleFactor { get; set; }

        /// <summary>
        /// 获取或设置比例因子偏移量。
        /// 设置测量的起始点。
        /// </summary>
        [FieldOrder(19)]
        [FieldCount(nameof(AmountOf16BitChannels))]
        public float[] ScaleFactorOffset { get; set; }

        /// <summary>
        /// 获取或设置起始角度。
        /// 单位：1/10000°
        /// </summary>
        [FieldOrder(20)]
        public int StartAngle { get; set; }

        /// <summary>
        /// 获取或设置单个角步长的大小。
        /// 单位：1/10000°
        /// </summary>
        [FieldOrder(21)]
        public ushort AngularStepSize { get; set; }

        /// <summary>
        /// 获取或设置数据量。
        /// 定义测量输出上的项目数。
        /// </summary>
        [FieldOrder(22)]
        public ushort AmountOfData { get; set; }

        /// <summary>
        /// 获取或设置数据流。
        /// 从Data_1到Data_n的数据流。
        /// </summary>
        [FieldOrder(23)]
        [FieldCount(nameof(AmountOfData))]
        public ushort[] Data { get; set; }

        /// <summary>
        /// 获取或设置8位通道数量。
        /// 输出测量数据的8位通道数。
        /// </summary>
        [FieldOrder(24)]
        public ushort AmountOf8BitChannels { get; set; }

        /// <summary>
        /// 获取或设置8位输出通道内容。
        /// 定义8位输出通道的内容，如RSSI1等。
        /// 仅当AmountOf8BitChannels > 0时存在。
        /// </summary>
        [FieldOrder(25)]
        [FieldCount(nameof(AmountOf8BitChannels))]
        [SerializeWhen(nameof(AmountOf8BitChannels), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public string[] OutputChannel8Bit { get; set; }

        /// <summary>
        /// 获取或设置8位通道的比例因子。
        /// 仅当AmountOf8BitChannels > 0时存在。
        /// </summary>
        [FieldOrder(26)]
        [FieldCount(nameof(AmountOf8BitChannels))]
        [SerializeWhen(nameof(AmountOf8BitChannels), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public float[] ScaleFactor8Bit { get; set; }

        /// <summary>
        /// 获取或设置8位通道的比例因子偏移量。
        /// 仅当AmountOf8BitChannels > 0时存在。
        /// </summary>
        [FieldOrder(27)]
        [FieldCount(nameof(AmountOf8BitChannels))]
        [SerializeWhen(nameof(AmountOf8BitChannels), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public float[] ScaleFactorOffset8Bit { get; set; }

        /// <summary>
        /// 获取或设置8位通道数据。
        /// 仅当AmountOf8BitChannels > 0时存在。
        /// </summary>
        [FieldOrder(28)]
        [FieldCount(nameof(AmountOfData))]
        [SerializeWhen(nameof(AmountOf8BitChannels), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public byte[] Data8Bit { get; set; }

        /// <summary>
        /// 获取或设置位置数据标志。
        /// 0表示无位置数据，1表示有位置数据。
        /// </summary>
        [FieldOrder(29)]
        public ushort Position { get; set; }

        /// <summary>
        /// 获取或设置位置数据。
        /// 仅当Position = 1时存在。
        /// </summary>
        [FieldOrder(30)]
        [SerializeWhen(nameof(Position), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public PositionData PositionInfo { get; set; }

        /// <summary>
        /// 获取或设置设备名称标志。
        /// 0表示无名称，1表示有名称。
        /// </summary>
        [FieldOrder(31)]
        public ushort Name { get; set; }

        /// <summary>
        /// 获取或设置设备名称。
        /// 仅当Name = 1时存在。
        /// </summary>
        [FieldOrder(32)]
        [SerializeWhen(nameof(Name), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public DeviceNameData DeviceNameInfo { get; set; }

        /// <summary>
        /// 获取或设置注释标志。
        /// 0表示无注释，1表示有注释。
        /// </summary>
        [FieldOrder(33)]
        public ushort Comment { get; set; }

        /// <summary>
        /// 获取或设置注释数据。
        /// 仅当Comment = 1时存在。
        /// </summary>
        [FieldOrder(34)]
        [SerializeWhen(nameof(Comment), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public CommentData CommentInfo { get; set; }

        /// <summary>
        /// 获取或设置时间标志。
        /// 0表示无时间戳，1表示有时间戳。
        /// </summary>
        [FieldOrder(35)]
        public ushort Time { get; set; }

        /// <summary>
        /// 获取或设置时间数据。
        /// 仅当Time = 1时存在。
        /// </summary>
        [FieldOrder(36)]
        [SerializeWhen(nameof(Time), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public TimeData TimeInfo { get; set; }

        /// <summary>
        /// 获取或设置事件信息标志。
        /// 0表示无事件信息，1表示有事件信息。
        /// </summary>
        [FieldOrder(37)]
        public ushort EventInfo { get; set; }

        /// <summary>
        /// 获取或设置事件信息数据。
        /// 仅当EventInfo = 1时存在。
        /// </summary>
        [FieldOrder(38)]
        [SerializeWhen(nameof(EventInfo), (ushort)0, BindingMode = BindingMode.OneWayToSource)]
        public EventInfoData EventInfoData { get; set; }
    }

    /// <summary>
    /// 表示位置数据。
    /// </summary>
    public class PositionData
    {
        /// <summary>
        /// 获取或设置位置计数器。
        /// </summary>
        [FieldOrder(0)]
        public uint PositionCounter { get; set; }
    }

    /// <summary>
    /// 表示设备名称数据。
    /// </summary>
    public class DeviceNameData
    {
        /// <summary>
        /// 获取或设置名称长度。
        /// </summary>
        [FieldOrder(0)]
        public ushort NameLength { get; set; }

        /// <summary>
        /// 获取或设置设备名称。
        /// </summary>
        [FieldOrder(1)]
        [FieldLength(nameof(NameLength))]
        public string DeviceName { get; set; }
    }

    /// <summary>
    /// 表示注释数据。
    /// </summary>
    public class CommentData
    {
        /// <summary>
        /// 获取或设置注释长度。
        /// </summary>
        [FieldOrder(0)]
        public ushort CommentLength { get; set; }

        /// <summary>
        /// 获取或设置注释内容。
        /// </summary>
        [FieldOrder(1)]
        [FieldLength(nameof(CommentLength))]
        public string CommentText { get; set; }
    }

    /// <summary>
    /// 表示时间数据。
    /// </summary>
    public class TimeData
    {
        /// <summary>
        /// 获取或设置年份。
        /// </summary>
        [FieldOrder(0)]
        public ushort Year { get; set; }

        /// <summary>
        /// 获取或设置月份。
        /// </summary>
        [FieldOrder(1)]
        public ushort Month { get; set; }

        /// <summary>
        /// 获取或设置日期。
        /// </summary>
        [FieldOrder(2)]
        public ushort Day { get; set; }

        /// <summary>
        /// 获取或设置小时。
        /// </summary>
        [FieldOrder(3)]
        public ushort Hour { get; set; }

        /// <summary>
        /// 获取或设置分钟。
        /// </summary>
        [FieldOrder(4)]
        public ushort Minute { get; set; }

        /// <summary>
        /// 获取或设置秒钟。
        /// </summary>
        [FieldOrder(5)]
        public ushort Second { get; set; }

        /// <summary>
        /// 获取或设置微秒。
        /// </summary>
        [FieldOrder(6)]
        public uint Microsecond { get; set; }
    }

    /// <summary>
    /// 表示事件信息数据。
    /// </summary>
    public class EventInfoData
    {
        /// <summary>
        /// 获取或设置事件类型。
        /// </summary>
        [FieldOrder(0)]
        public ushort EventType { get; set; }

        /// <summary>
        /// 获取或设置编码器位置。
        /// </summary>
        [FieldOrder(1)]
        public uint EncoderPosition { get; set; }

        /// <summary>
        /// 获取或设置事件时间（微秒）。
        /// </summary>
        [FieldOrder(2)]
        public uint EventTime { get; set; }

        /// <summary>
        /// 获取或设置事件角度。
        /// 单位：1/10000°
        /// </summary>
        [FieldOrder(3)]
        public int EventAngle { get; set; }
    }
} 