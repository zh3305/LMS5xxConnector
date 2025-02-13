using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LCMstateModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        public string LCMState { get; set; }
    }
} 