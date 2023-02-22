using BinarySerialization;

namespace LMS5xxConnector
{
    public enum YesNo
    {
        [SerializeAsEnum("0")] No,
        [SerializeAsEnum("1")] Yes,
    }
}