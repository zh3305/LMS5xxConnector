using BinarySerialization;

namespace LMS5xxConnector
{
    public enum SuccessError : int
    {
        [SerializeAsEnum("0")] Error = 0,
        [SerializeAsEnum("1")] Success = 1
    }
}