using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LMPoutputRangeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        public string OutputRange { get; set; }
    }
} 