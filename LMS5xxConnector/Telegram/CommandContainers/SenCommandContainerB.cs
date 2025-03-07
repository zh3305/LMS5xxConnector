using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SenCommandContainerB : CommandContainerBaseB
    {
        public override CommandBaseB CommandConnent { get; set; }
    } 
    public class SeaCommandContainerB : CommandContainerBaseB
    {
        [Subtype("Command", Commands.LMDscandata, typeof(ConfirmationRequestModeCommandBaseB), BindingMode = BindingMode.OneWay)]

        public override CommandBaseB CommandConnent { get; set; }
    }
} 