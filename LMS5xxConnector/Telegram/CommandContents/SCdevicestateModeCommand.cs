using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class SCdevicestateModeCommand : CommandBase
    {
        // Busy / logged-in: 0
        // Ready: 1
        // Error: 2
        // Standby: 3
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string StatusCode { get; set; }
    }
}