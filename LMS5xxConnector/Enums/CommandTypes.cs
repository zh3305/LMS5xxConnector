using BinarySerialization;

namespace LMS5xxConnector
{
    /// <summary>
    /// 表示 CoLa A 通讯协议中的命令类型。
    /// 每个枚举值对应一个特定的命令或应答类型，用于与SICK激光扫描仪等设备进行通信。
    /// </summary>
    public enum CommandTypes
    {
        /// <summary>
        /// 读取命令 (Read)。
        /// 用于从设备读取数据或状态信息，例如读取设备名称、配置参数等。
        /// </summary>
        [SerializeAsEnum("sRN")]
        Srn,  // Read Name

        /// <summary>
        /// 写入命令 (Write)。
        /// 用于向设备写入配置或设置，例如设置IP地址、波特率、测量参数等。
        /// </summary>
        [SerializeAsEnum("sWN")]
        Swn,  // Write Name

        /// <summary>
        /// 方法命令 (Method)。
        /// 用于执行特定的操作或方法，例如开始测量、停止测量、重启设备等。
        /// </summary>
        [SerializeAsEnum("sMN")]
        Smn,  // Method Name

        /// <summary>
        /// 事件命令 (Event)。
        /// 由设备发送，表示某个事件的发生，例如错误发生、警告触发等。
        /// </summary>
        [SerializeAsEnum("sEN")]
        Sen,  // Event Notification

        /// <summary>
        /// 读取应答 (Answer to sRN)。
        /// 设备对 `sRN` 读取命令的响应，包含请求的数据或状态信息。
        /// </summary>
        [SerializeAsEnum("sRA")]
        Sra,  // Read Answer

        /// <summary>
        /// 写入应答 (Answer to sWN)。
        /// 设备对 `sWN` 写入命令的响应，表示写入操作是否成功。
        /// </summary>
        [SerializeAsEnum("sWA")]
        Swa,  // Write Answer

        /// <summary>
        /// 事件应答 (Answer to sEN)。
        /// 设备对 `sEN` 事件命令的响应，通常包含事件的详细信息或确认。
        /// </summary>
        [SerializeAsEnum("sEA")]
        Sea,  // Event Answer

        /// <summary>
        /// 状态查询 (Status Query)。
        /// 用于查询设备的状态信息，例如当前运行状态、错误状态等。
        /// </summary>
        [SerializeAsEnum("sSN")]
        Ssn,  // Status Query

        /// <summary>
        /// 通用应答 (General Answer)。
        /// 用于响应其他类型的命令，例如对 `sMN` 方法命令的响应。
        /// </summary>
        [SerializeAsEnum("sAN")]
        San,  // General Answer

        /// <summary>
        /// 错误代码 (Error Code)。
        /// 当命令执行失败时，设备返回的错误代码，包含具体的错误信息，帮助诊断问题。
        /// </summary>
        [SerializeAsEnum("sFA")]
        Sfa   // Fault Answer
    }
}
