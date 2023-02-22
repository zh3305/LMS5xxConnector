using System.Security.Cryptography;
using System.Xml;
using System.ComponentModel;
using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContainers;

namespace LMS5xxConnector.Telegram
{
    public class TelegramContent
    {
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public CommandTypes CommandTypes { get; set; }

        [FieldOrder(1)]
        [Subtype("CommandTypes", CommandTypes.Smn, typeof(SmnCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sra, typeof(SraCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.San, typeof(SanCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Ssn, typeof(SsnCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sea, typeof(SenCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sen, typeof(SenCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sfa, typeof(SfaCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Swa, typeof(CommandContainerBase), BindingMode = BindingMode.OneWay)]
        public CommandContainerBase Payload { get; set; }
    }

}