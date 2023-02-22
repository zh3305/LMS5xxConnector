using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SenCommandContainer : CommandContainerBase
    {
        [SubtypeDefault(typeof(ConfirmationRequestModeCommandBase))]
        public override CommandBase CommandConnent { get; set; }
    }
}