using BinarySerialization;

namespace LMS5xxConnector.Enums;

/// <summary>
/// 表示设备的状态码。
/// 每个枚举值对应一个特定的设备状态，用于与设备进行通信时描述当前的设备状态。
/// </summary>
public enum DeviceStateCode
{
    /// <summary>
    /// 忙碌或已登录。
    /// 设备正在处理命令或处于登录状态。
    /// </summary>
    [SerializeAsEnum("0")]
    BusyLoggedIn = 0,

    /// <summary>
    /// 就绪。
    /// 设备已经准备好执行命令或测量。
    /// </summary>
    [SerializeAsEnum("1")]
    Ready = 1,

    /// <summary>
    /// 错误。
    /// 设备检测到错误，可能需要用户干预或重启设备。
    /// </summary>
    [SerializeAsEnum("2")]
    Error = 2,

    /// <summary>
    /// 待机。
    /// 设备处于待机模式，等待进一步指令。
    /// </summary>
    [SerializeAsEnum("3")]
    Standby = 3
}