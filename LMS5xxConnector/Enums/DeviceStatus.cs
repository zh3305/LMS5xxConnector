using BinarySerialization;

namespace LMS5xxConnector
{
    public enum DeviceStatus
    {
        [SerializeAsEnum("0 0 ")] Ok,
        [SerializeAsEnum("0 1 ")] Error,
        [SerializeAsEnum("0 2 ")] PollutionWarning,

        //Pollution error with nodevice error:  4
        [SerializeAsEnum("0 4 ")] PollutionErrorWithNodeviceError,

        // Pollution error with device error:  5
        [SerializeAsEnum("0 5 ")] PollutionErrorWithDeviceError,

        //Data output during laser of
        [SerializeAsEnum("1 0 ")] DataOutputDuringLaserOf
    }
}