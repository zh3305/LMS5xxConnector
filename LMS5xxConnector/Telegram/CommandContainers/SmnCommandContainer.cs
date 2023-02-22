using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SmnCommandContainer : CommandContainerBase
    {
        [Subtype("Command", Commands.SetAccessMode, typeof(SetAccessModeCommand))]
        public override CommandBase CommandConnent { get; set; }
    }
    public class SrnCommandContainer : CommandContainerBase
    {
    }
}