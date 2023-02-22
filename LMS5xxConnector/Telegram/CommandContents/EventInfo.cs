using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class EventInfo
    {
        //HasEventInfo
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public YesNo HasEventInfo { get; set; }

        [FieldOrder(1)]
        [Subtype("HasEventInfo", YesNo.Yes, typeof(EventInformation))]
        [SubtypeDefault(typeof(EmptyClass))]
        public EmptyClass TimeInfo { get; set; }
    }
}