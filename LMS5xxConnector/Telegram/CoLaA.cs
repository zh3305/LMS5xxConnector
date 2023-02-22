using BinarySerialization;

namespace LMS5xxConnector.Telegram
{
    public class PacketCoLaA
    {
        // Start of text<STX>  02 02 02 02 

        [FieldOrder(0)]
        public uint Length { get; set; }

        [FieldOrder(1)]
        [FieldLength("Length")]
        [FieldChecksum("Checksum", Mode = ChecksumMode.Xor)]
        public TelegramContent Content { get; set; }

        [FieldOrder(2)]
        public byte Checksum { get; set; }
    }
}