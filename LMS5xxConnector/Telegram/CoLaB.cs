using BinarySerialization;

namespace LMS5xxConnector.Telegram
{
    public class CoLaB
    {
        // Start of text<STX>  02 02 02 02 
        [FieldOrder(0)]
        [FieldLength(4)]
        public byte[] STX { get; set; } = [0x2,0x2,0x2,0x2];
        
        [FieldOrder(1)]
        [FieldEndianness(Endianness.Big)]
        public uint Length { get; set; }

        [FieldOrder(2)]
        [FieldLength(nameof(Length))]
        [FieldChecksum("Checksum", Mode = ChecksumMode.Xor)]
        public TelegramContentB Content { get; set; }

        [FieldOrder(3)]
        public byte Checksum { get; set; }
    }
}