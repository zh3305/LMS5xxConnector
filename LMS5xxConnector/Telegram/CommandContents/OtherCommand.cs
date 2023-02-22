using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    // public  const byte STX = 0x2;
    // public const byte ETX = 0x3;

    public class OtherCommand
    {
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Commands { get; set; }
    }
}