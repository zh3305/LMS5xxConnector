using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class SetAccessModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        public string AccessMode { get; set; }
    }
} 