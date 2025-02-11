using BinarySerialization;

namespace LMS5xxConnector.Enums;

/// <summary>
/// 表示数字输出状态的枚举
/// </summary>
public enum OutputStatusEnum
{
    /// <summary>
    /// 所有输出低电平
    /// 长度为2位: "0 0"
    /// 适用于所有传感器
    /// </summary>
    [SerializeAsEnum("0 0 ")]
    AllOutputsLow = 0,

    /// <summary>
    /// 所有输出高电平
    /// 长度为2位: "F 0"
    /// 适用于 TIM5xx, TIM7xx, LDxxx
    /// </summary>
    [SerializeAsEnum("F 0 ")]
    AllOutputsHigh_TIM5xx_TIM7xx_LDxxx = 0xF0,

    /// <summary>
    /// 所有输出高电平
    /// 长度为2位: "2 0"
    /// 适用于 TIM24x
    /// </summary>
    [SerializeAsEnum("2 0 ")]
    AllOutputsHigh_TIM24x = 0x20,

    
    /// <summary>
    /// 所有输出高电平（包括外部输出）
    /// 长度为4位: "07 FF"
    /// 适用于 LMS1xx
    /// </summary>
    [SerializeAsEnum("7 0 ")]
    AllinternalHighIncludingExternal_LMS1xx = 0x0700,


    /// <summary>
    /// 所有输出高电平（包括外部输出）
    /// 长度为4位: "07 FF"
    /// 适用于 LMS1xx
    /// </summary>
    [SerializeAsEnum("07 FF ")]
    AllOutputsHighIncludingExternal_LMS1xx = 0x07FF,

    // /// <summary>
    // /// 所有内部输出高电平
    // /// 长度为2位: "3F 0"
    // /// 适用于 LMS1xx
    // /// </summary>
    // [SerializeAsEnum("3F 0 ")]
    // AllInternalOutputsHigh_LMS1xx = 0x3F00,
    /// <summary>
    /// 所有内部输出高电平
    /// 长度为2位: "3F 0"
    /// 适用于 LMS5xx
    /// </summary>
    [SerializeAsEnum("3F 0 ")]
    AllInternalOutputsHigh_LMS5xx = 0x3F00,

    /// <summary>
    /// 所有输出高电平（包括外部输出）
    /// 长度为4位: "3F FF"
    /// 适用于 LMS5xx
    /// </summary>
    [SerializeAsEnum("3F FF ")]
    AllOutputsHighIncludingExternal_LMS5xx = 0x3FFF,

    /// <summary>
    /// 输出1高电平
    /// 长度为2位: "1 0"
    /// 适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("1 0 ")]
    Output1High_LMS4000 = 0x10,

    // /// <summary>
    // /// 输出2高电平
    // /// 长度为2位: "2 0"
    // /// 适用于 LMS4000
    // /// </summary>
    // [SerializeAsEnum("2 0")]
    // Output2High_LMS4000 = 0x20,

    // /// <summary>
    // /// 输出1和输出2都高电平
    // /// 长度为2位: "3 0"
    // /// 适用于 LMS4000
    // /// </summary>
    // [SerializeAsEnum("3 0")]
    // Output1And2High_LMS4000 = 0x30,

    /// <summary>
    /// 输出4高电平
    /// 长度为2位: "8 0"
    /// 适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("8 0 ")]
    Output4High_LMS4000 = 0x80,

    /// <summary>
    /// 输出1和输出4都高电平
    /// 长度为2位: "9 0"
    /// 适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("9 0 ")]
    Output1And4High_LMS4000 = 0x90,

    /// <summary>
    /// 输出2和输出4都高电平
    /// 长度为2位: "A 0"
    /// 适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("A 0 ")]
    Output2And4High_LMS4000 = 0xA0,

    /// <summary>
    /// 输出1、输出2和输出4都高电平
    /// 长度为2位: "B 0"
    /// 适用于 LMS4000
    /// </summary>
    [SerializeAsEnum("B 0 ")]
    Output1And2And4High_LMS4000 = 0xB0,

}