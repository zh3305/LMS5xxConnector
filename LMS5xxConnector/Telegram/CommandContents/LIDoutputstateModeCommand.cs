using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{

    /// <summary>
    /// 用于处理每次输出状态改变时发送的输出报文的命令模式类
    /// 该类包含每次输出状态改变时发送的输出报文的相关信息，符合文档中的 sRA LIDoutputstate 响应结构
    /// 此类是只读的，用于解析从设备读取的输出状态信息
    /// </summary>
    public class LIDoutputstateModeCommand : CommandBase
    {
        /// <summary>
        /// 获取或设置状态版本号
        /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string StatusVersionNumber { get;  set; }

        /// <summary>
        /// 获取或设置系统计数器
        /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符
        /// 系统计数器表示自设备上电以来的时间（以微秒为单位），最大值为 71 分钟，然后重新从 0 开始计数
        /// </summary>
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string StatusSystemCounter { get;  set; }

        /// <summary>
        /// 获取或设置输出列表
        /// 该字段包含 17 个输出状态对象，每个对象表示一个输出的状态和计数值
        /// </summary>
        [FieldOrder(2)]
        [FieldCount(17)]
        public List<OutputState> Outputs { get; set; } = new List<OutputState>();

        /// <summary>
        /// 获取或设置时间结构
        /// 该字段表示每次输出状态改变时发送的输出报文的时间信息
        /// </summary>
        [FieldOrder(3)]
        public TimeStruct Time { get; set; }

        /// <summary>
        /// 表示单个输出状态的对象
        /// </summary>
        public class OutputState
        {
            /// <summary>
            /// 获取或设置输出状态
            /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符
            /// 输出状态可以是：未激活 (0), 激活 (1), 输出未使用 (2)
            /// </summary>
            [FieldOrder(0)]
            [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
            public string State { get; set; }

            /// <summary>
            /// 获取或设置输出计数
            /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符
            /// 输出计数表示输出状态变化的次数
            /// </summary>
            [FieldOrder(1)]
            [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
            public string Count { get; set; }
        }
    }
}