using BinarySerialization;

namespace LMS5xxConnector
{
    public enum StopStart
    {
        [SerializeAsEnum("0")] Stop,
        [SerializeAsEnum("1")] Start
    }
}