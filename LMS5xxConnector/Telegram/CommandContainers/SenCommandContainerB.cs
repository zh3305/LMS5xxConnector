using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SenCommandContainerB : CommandContainerBaseB
    {
        // [SubtypeDefault(typeof(ConfirmationRequestModeCommandBaseB))]
        public override CommandBaseB CommandConnent { get; set; }
    }
} 