using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class CommandBaseExecutionResultModeCommand : CommandBase
    {
        [SerializeAs(SerializedType.TerminatedString)]
        public SuccessError SuccessError { get; set; }
    }
}