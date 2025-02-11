using BinarySerialization;

namespace LMS5xxConnector.Enums;

/// <summary>
/// 表示数字输入状态的枚举
/// </summary>
public enum InputStatusEnum
{
    /// <summary>
    /// 所有输入低电平
    /// 长度为2位: "0 0"
    /// 适用于 LMS1xx, LMS5xx, LMS4000, TiM24x, MRS 1000, LMS1000, MRS6000, LRS4000
    /// </summary>
    [SerializeAsEnum("0 0 ")]
    AllLow = 0,

    /// <summary>
    /// 所有输入高电平
    /// 长度为2位: "3 0"
    /// 适用于 LMS1xx, LMS5xx, LMS4000
    /// </summary>
    [SerializeAsEnum("3 0 ")]
    AllHigh = 3,

    /// <summary>
    /// LMS4000 输入1高电平，输入2低电平
    /// 长度为2位: "1 0"
    /// 仅适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("1 0 ")]
    Input1HighLMS4000 = 1,

    /// <summary>
    /// LMS4000 输入1低电平，输入2高电平
    /// 长度为2位: "2 0"
    /// 仅适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("2 0 ")]
    Input2HighLMS4000 = 2,

    // /// <summary>
    // /// LMS4000 输入1和输入2都高电平
    // /// 长度为2位: "3 0"
    // /// 仅适用于 LMS4000
    // /// </summary>
    // [SerializeAsEnum("30")]
    // Input1And2HighLMS4000 = 3
}