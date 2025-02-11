using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContainers;

namespace LMS5xxConnector.Telegram
{
    public class TelegramContentB
    {
        [FieldOrder(0)]
        public CommandTypes CommandTypes { get; set; }

        // [FieldOrder(1)]
        // public CommandContainerB Payload { get; set; }
    }
} 