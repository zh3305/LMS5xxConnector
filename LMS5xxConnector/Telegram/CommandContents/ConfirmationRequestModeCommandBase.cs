using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class ConfirmationRequestModeCommandBase : CommandBase
    {
        [SerializeAs(SerializedType.TerminatedString)]
        [FieldLength(1)]
        public StopStart StopStart { get; set; }
    }
}