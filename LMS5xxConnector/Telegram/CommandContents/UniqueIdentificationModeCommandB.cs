using BinarySerialization;
using System.Text;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class UniqueIdentificationModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        [FieldEndianness(Endianness.Big)]
        public ushort DeviceDesignationLength { get; set; }

        [FieldOrder(1)]
        [FieldLength(nameof( DeviceDesignationLength))]
        public string DeviceDesignation { get; set; }

        [FieldOrder(2)]
        [FieldEndianness(Endianness.Big)]
        public ushort FirmwareVersionLength { get; set; }

        [FieldOrder(3)]
        [FieldLength(nameof( FirmwareVersionLength))]
        public string FirmwareVersion { get; set; }
    }
} 