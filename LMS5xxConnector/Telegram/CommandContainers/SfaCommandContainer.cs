using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    public class SfaCommandContainer : CommandContainerBase
    {
        [Ignore] public override Commands Command { get; set; } = Commands.ErrorCode;
        [Ignore] public override CommandBase CommandConnent { get; set; }
        [FieldOrder(0)] public SickValue<ushort> ErrorCode { get; set; }
    }
}