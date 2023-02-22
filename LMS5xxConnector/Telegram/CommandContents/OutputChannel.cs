using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class OutputChannel
    {
        // DIST1：第一个脉冲的距离值
        // DIST2：第二个脉冲的距离值
        // DIST3：第三个脉冲的距离值
        // DIST4：第四个脉冲的距离值
        // DIST5：第五个脉冲的距离值
        // RSSI1：第一个脉冲的能量值
        // RSSI2：第二个脉冲的能量值
        // RSSI3 ：第三个脉冲的能量值
        // RSSI4：第四个脉冲的能量值
        // RSSI5：第五个脉冲的能量值
        //定义输出通道的内容 径向距离值 (DIST) 的单位是 mm
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string ChannelContent { get; set; }

        // ScaleFactor
        //测量值的比例因子或因子（对于 LMS5xx，这取决于角分辨率）
        // Factor × 1: 3F800000h
        // Factor × 2: 40000000h
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<float> ScaleFactor { get; set; }

        /// <summary>
        /// 设置测量起点
        /// </summary>
        // Scale factor offset
        //LMS5xx 0
        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string ScaleFactorOffset { get; set; }


        // Start angle
        // 起始角度
        // [1/10000°]
        [FieldOrder(3)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<int> StartAngle { get; set; }

        // Size of single angular step
        //角分辨率
        // [1/10000°]
        [FieldOrder(4)]
        // [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> AngularStepSize { get; set; }

        //Amount of data
        //数据量
        [FieldOrder(5)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> DataAmount { get; set; }

        //数据列表
        [FieldOrder(6)]
        [FieldCount(nameof(DataAmount) + "." + nameof(SickValue<ushort>.Value))]
        public List<SickValue<ushort>> DistDatas { get; set; }
    }
}