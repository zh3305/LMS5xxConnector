using BinarySerialization;
using System.Text;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class UniqueIdentificationModeCommandB : CommandBaseB
    {
        [FieldOrder(0)]
        [FieldEndianness(Endianness.Big)]
        public ushort NameLength { get; set; }

        [FieldOrder(1)]
        [FieldLength(nameof( NameLength))]
        public string Name { get; set; }

        [FieldOrder(2)]
        [FieldEndianness(Endianness.Big)]
        public ushort VersionLength { get; set; }

        [FieldOrder(3)]
        [FieldLength(nameof( VersionLength))]
        public string Version { get; set; }
    }
} 