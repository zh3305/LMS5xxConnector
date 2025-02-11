using BinarySerialization;

namespace LMS5xxConnector.Enums;

/// <summary>
/// 污染状态。
/// </summary>
public enum LCMStateCode
{
    /// <summary>
    /// 无污染。
    /// 设备处于正常工作状态，没有检测到污染。
    /// </summary>
    [SerializeAsEnum("0")]
    NoContamination = 0,

    /// <summary>
    /// 污染警告。
    /// 设备检测到轻微污染，建议进行清洁。
    /// </summary>
    [SerializeAsEnum("1")]
    ContaminationWarning = 1,

    /// <summary>
    /// 污染错误。
    /// 设备检测到严重污染，可能需要立即清洁或维护。
    /// </summary>
    [SerializeAsEnum("2")]
    ContaminationError = 2,

    /// <summary>
    /// 测量功能缺陷。
    /// 设备的测量功能受到污染影响，无法正常工作。
    /// </summary>
    [SerializeAsEnum("3")]
    MeasurementFunctionalityDefective = 3
}