using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class TimeStruct
    {
        //HasTime
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public YesNo HasTime { get; set; }


        [FieldOrder(1)]
        [Subtype("HasTime", YesNo.Yes, typeof(TimeInfo))]
        [SubtypeDefault(typeof(EmptyClass))]
        public EmptyClass TimeInfo { get; set; }
    }
}