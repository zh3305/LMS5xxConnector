using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    /// <summary>
    /// 用于处理设备状态的命令模式类。
    /// 该类包含设备的状态码 (Status code)，符合文档中的 sRA SCdevicestate 响应结构。
    /// 此类是只读的，用于解析从设备读取的设备状态信息
    /// </summary>
    public class SCdevicestateModeCommand : CommandBase
    {
        /// <summary>
        /// 获取设备状态码。
        /// 该字段在序列化时以字符串形式表示，并使用 <see cref="DeviceStateCode"/> 枚举的序列化值。
        /// 此字段是只读的，表示从设备读取的设备状态。
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public DeviceStateCode StateCode { get; private set; }
    }


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
}