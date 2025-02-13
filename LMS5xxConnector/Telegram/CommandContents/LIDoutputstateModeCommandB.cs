using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LIDoutputstateModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        public byte[] OutputState { get; set; }
    }
} 