using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    /// <summary>
    /// 用于处理设备唯一识别信息的命令模式类。
    /// </summary>
    public class UniqueIdentificationModeCommand : CommandBase
    {
        /// <summary>
        /// 获取或设置名称长度。
        /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符。
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string NameLength { get; set; }

        /// <summary>
        /// 设备系列的固件名称。
        /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符。
        /// </summary>
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置版本长度。
        /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符。
        /// </summary>
        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string VersionLength { get; set; }

        /// <summary>
        /// 固件版本。
        /// </summary>
        [FieldOrder(4)]
        public string Version { get; set; }
    }
}