using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContainers;

namespace LMS5xxConnector.Telegram
{
    /// <summary>
    /// 表示 CoLa B 通讯协议中的报文内容。
    /// 该类用于封装命令类型及其对应的负载（Payload），并支持不同类型命令的序列化和反序列化。
    /// </summary>
    public class TelegramContentB
    {
        /// <summary>
        /// 获取或设置命令类型。
        /// 命令类型决定了报文的具体功能，例如读取、写入、执行方法等。
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public CommandTypes CommandTypes { get; set; }

        /// <summary>
        /// 获取或设置命令的负载（Payload）。
        /// 根据不同的命令类型，负载可以是不同类型的对象，具体取决于命令的功能。
        /// </summary>
        [FieldOrder(1)]
        [Subtype("CommandTypes", CommandTypes.Srn, typeof(SrnCommandContainerB), BindingMode = BindingMode.OneWay)]
        // [Subtype("CommandTypes", CommandTypes.Swn, typeof(SwnCommandContainerB), BindingMode = BindingMode.OneWay)]
        // [Subtype("CommandTypes", CommandTypes.Smn, typeof(SmnCommandContainerB), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sen, typeof(SenCommandContainerB), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sra, typeof(SraCommandContainerB), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Swa, typeof(SenCommandContainerB), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sea, typeof(SeaCommandContainerB), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Ssn, typeof(SsnCommandContainerB), BindingMode = BindingMode.OneWay)]
        // [Subtype("CommandTypes", CommandTypes.San, typeof(SanCommandContainerB), BindingMode = BindingMode.OneWay)]
        // [Subtype("CommandTypes", CommandTypes.Sfa, typeof(SfaCommandContainerB), BindingMode = BindingMode.OneWay)]
        public CommandContainerBaseB Payload { get; set; }
    }
} 