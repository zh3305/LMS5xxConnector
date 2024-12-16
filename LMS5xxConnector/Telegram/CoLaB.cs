using BinarySerialization;

namespace LMS5xxConnector.Telegram
{
    public class CoLaB
    {
        // Start of text<STX>  02 02 02 02 
        [FieldOrder(0)]
        public byte[] STX { get; set; } = [0x2,0x2,0x2,0x2];
        [FieldOrder(1)]
        public uint Length { get; set; }

        [FieldOrder(2)]
        [FieldLength("Length")]
        [FieldChecksum("Checksum", Mode = ChecksumMode.Xor)]
        public TelegramContent Content { get; set; }

        [FieldOrder(3)]
        public byte Checksum { get; set; }
    }
}