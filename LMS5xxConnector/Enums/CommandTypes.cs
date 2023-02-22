using BinarySerialization;

namespace LMS5xxConnector
{
    public enum CommandTypes
    {
        //Read
        [SerializeAsEnum("sRN")]
        Srn,

        //Write
        [SerializeAsEnum("sWN")]
        Swn,

        //Method
        [SerializeAsEnum("sMN")]
        Smn,

        //Event
        [SerializeAsEnum("sEN")]
        Sen,

        //Answer
        [SerializeAsEnum("sRA")]
        Sra,
        //Answer
        [SerializeAsEnum("sWA")]
        Swa,
        //Answer
        [SerializeAsEnum("sEA")]
        Sea,
        //Answer
        [SerializeAsEnum("sSN")]
        Ssn,
        //Answer
        [SerializeAsEnum("sAN")]
        San,
        //ErrorCode
        [SerializeAsEnum("sFA")]
        Sfa
    }
}
