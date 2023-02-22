using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SsnCommandContainer : CommandContainerBase
    {
        [Subtype("Command", Commands.LMDscandata, typeof(LMDscandataModeCommand))]
        [Subtype("Command", Commands.LIDoutputstate, typeof(LIDoutputstateModeCommand))]
        public override CommandBase CommandConnent { get; set; }
    }
}