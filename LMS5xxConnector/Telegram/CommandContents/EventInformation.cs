using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class EventInformation : EmptyClass
    {
        //Type
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string Type { get; set; }

        //Encoder position
        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string EncoderPosition { get; set; }

        //Time of event
        [FieldOrder(2)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string TimeOfEvent { get; set; }

        //Angle of event
        [FieldOrder(3)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string AngleOfEvent { get; set; }
    }
}