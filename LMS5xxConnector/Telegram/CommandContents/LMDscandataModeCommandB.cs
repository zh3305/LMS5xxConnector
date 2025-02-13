using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LMDscandataModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        public byte[] ScanData { get; set; }
    }
} 