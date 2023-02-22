using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SanCommandContainer : CommandContainerBase
    {
        [Subtype("Command", Commands.mSCreboot, typeof(CommandBase), BindingMode = BindingMode.OneWay)]
        [SubtypeDefault(typeof(CommandBaseExecutionResultModeCommand))]
        public override CommandBase CommandConnent { get; set; }
    }
}