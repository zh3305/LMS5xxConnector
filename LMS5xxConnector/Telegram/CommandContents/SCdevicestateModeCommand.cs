using BinarySerialization;
using LMS5xxConnector.Enums;

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
}