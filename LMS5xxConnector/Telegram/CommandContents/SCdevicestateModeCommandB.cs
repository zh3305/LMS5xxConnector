using BinarySerialization;
using LMS5xxConnector.Enums;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class SCdevicestateModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        public DeviceStateCode StateCode { get; set; }
    }
} 