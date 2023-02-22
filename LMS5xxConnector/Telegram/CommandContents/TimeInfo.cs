using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class TimeInfo : EmptyClass
    {
        //Year
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Year { get; set; }

        //Month
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Month { get; set; }

        //Day
        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Day { get; set; }

        //Hour
        [FieldOrder(3)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Hour { get; set; }

        //Minute
        [FieldOrder(4)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Minute { get; set; }

        //Second
        [FieldOrder(5)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Second { get; set; }

        //Millisecond
        [FieldOrder(6)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Millisecond { get; set; }
    }
}