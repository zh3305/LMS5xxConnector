using BinarySerialization;

namespace LMS5xxConnector.Enums;

/// <summary>
/// 表示设备状态的枚举
/// </summary>
public enum DeviceStatusEnum
{
    /// <summary>
    /// 所有设备正常运行
    /// 长度为2位: "0 0"
    /// </summary>
    [SerializeAsEnum("0 0 ")]
    AllOk = 0,

    /// <summary>
    /// 所有设备错误
    /// 长度为2位: "0 1"
    /// </summary>
    [SerializeAsEnum("0 1 ")]
    AllError = 1,

    /// <summary>
    /// LMS1xx 和 LMS5xx 污染警告
    /// 长度为2位: "0 2"
    /// 仅适用于 LMS1xx 和 LMS5xx
    /// </summary>
    [SerializeAsEnum("0 2 ")]
    PollutionWarningLMS1xxLMS5xx = 2,

    /// <summary>
    /// LMS1xx 和 LMS5xx 污染错误，无设备错误
    /// 长度为2位: "0 4"
    /// 仅适用于 LMS1xx 和 LMS5xx
    /// </summary>
    [SerializeAsEnum("0 4 ")]
    PollutionErrorNoDeviceErrorLMS1xxLMS5xx = 4,

    /// <summary>
    /// LMS1xx 和 LMS5xx 污染错误，伴有设备错误
    /// 长度为2位: "0 5"
    /// 仅适用于 LMS1xx 和 LMS5xx
    /// </summary>
    [SerializeAsEnum("0 5 ")]
    PollutionErrorWithDeviceErrorLMS1xxLMS5xx = 5,

    /// <summary>
    /// LMS4000 激光关闭时的数据输出
    /// 长度为2位: "1 0"
    /// 仅适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("1 0 ")]
    DataOutputDuringLaserOffLMS4000 = 10
}