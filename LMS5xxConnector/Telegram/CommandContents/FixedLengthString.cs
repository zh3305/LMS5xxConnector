using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class FixedLengthString
    {
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public SickValue<ushort> Length { get; set; }

        [FieldOrder(1)]
        [FieldLength(nameof(Length) + "." + nameof(SickValue<ushort>.Value))]
        public string Value { get; set; }
    }
}