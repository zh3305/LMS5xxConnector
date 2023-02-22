using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SraCommandContainer : CommandContainerBase
    {
        [Subtype("Command", Commands.DeviceIdentOldCode, typeof(UniqueIdentificationModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.DeviceIdent, typeof(UniqueIdentificationModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LCMstate, typeof(LCMstateModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.SCdevicestate, typeof(SCdevicestateModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LIDoutputstate, typeof(LIDoutputstateModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LMDscandata, typeof(LMDscandataModeCommand), BindingMode = BindingMode.OneWay)]
        public override CommandBase CommandConnent { get; set; }
    }
}