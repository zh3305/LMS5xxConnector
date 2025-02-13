using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public abstract class CommandContainerBaseB
    {
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public virtual Commands Command { get; set; }
        
        [FieldOrder(1)]
        public virtual CommandBaseB CommandConnent { get; set; }
    }
} 