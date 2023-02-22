using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class UniqueIdentificationModeCommand : CommandBase
    {
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string NameLength { get; set; }

        /// <summary>
        /// 设备系列的固件名称
        /// </summary>
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Name { get; set; }

        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string VersionLength { get; set; }

        /// <summary>
        /// 固件版本
        /// </summary>
        [FieldOrder(4)]
        public string Version { get; set; }
    }
}