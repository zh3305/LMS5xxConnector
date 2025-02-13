using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SsnCommandContainerB : CommandContainerBaseB
    {
        [Subtype("Command", Commands.LMDscandata, typeof(LMDscandataModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LIDoutputstate, typeof(LIDoutputstateModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.SCdevicestate, typeof(SCdevicestateModeCommandB), BindingMode = BindingMode.OneWay)]
        public override CommandBaseB CommandConnent { get; set; }
    }
} 