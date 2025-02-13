using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SwnCommandContainerB : CommandContainerBaseB
    {
        [Subtype("Command", Commands.SetAccessMode, typeof(SetAccessModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LMPoutputRange, typeof(LMPoutputRangeCommandB), BindingMode = BindingMode.OneWay)]
        public override CommandBaseB CommandConnent { get; set; }
    }
} 