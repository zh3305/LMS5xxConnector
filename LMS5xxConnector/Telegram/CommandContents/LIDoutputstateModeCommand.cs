using System.Collections.Generic;
using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LIDoutputstateModeCommand : CommandBase
    {
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string StatusVersionNumber { get; set; }

        [FieldOrder(1)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string StatusSystemCounter { get; set; }

        [FieldOrder(2)]
        [FieldCount(17)]
        public List<LIDoutputstateOutputs> Outputs { get; set; }


        //Time
        [FieldOrder(3)]
        public TimeStruct Time { get; set; }


        public class LIDoutputstateOutputs
        {
            [FieldOrder(0)]
            [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
            public string State { get; set; }



            [FieldOrder(1)]
            [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
            public string Count { get; set; }
        }

    }
}