using BinarySerialization;

namespace LMS5xxConnector
{
    /// <summary>
    /// 表示 CoLa A 通讯协议中的具体命令。
    /// 每个枚举值对应一个特定的命令或操作，用于与SICK激光扫描仪等设备进行通信。
    /// </summary>
    public enum Commands
    {
        /// <summary>
        /// 设置访问模式。
        /// </summary>
        SetAccessMode,

        /// <summary>
        /// 设置扫描配置。
        /// </summary>
        [SerializeAsEnum("mLMPsetscancfg")]
        SetScanConfig,

        /// <summary>
        /// 配置扫描数据。
        /// </summary>
        [SerializeAsEnum("LMDscandatacfg")]
        ScanDataConfig,

        /// <summary>
        /// 获取设备信息（旧编码）。
        /// </summary>
        [SerializeAsEnum("0")]
        DeviceIdentOldCode,

        /// <summary>
        /// 获取设备信息。
        /// </summary>
        [SerializeAsEnum("DeviceIdent")]
        DeviceIdent, // DeviceIdent == "0"

        /// <summary>
        /// 设置时间戳。
        /// </summary>
        LSPsetdatetime,

        /// <summary>
        /// 运行命令。
        /// </summary>
        Run,

        /// <summary>
        /// 获取扫描数据。
        /// </summary>
        LMDscandata,

        /// <summary>
        /// 设置输出状态。
        /// </summary>
        mDOSetOutput,

        /// <summary>
        /// 重置输出计数器。
        /// </summary>
        LIDrstoutpcnt,

        /// <summary>
        /// 重启设备。
        /// </summary>
        mSCreboot,

        /// <summary>
        /// 读取设备状态。
        /// </summary>
        SCdevicestate,

        /// <summary>
        /// 每次输出状态改变时发送输出报文。
        /// </summary>
        LIDoutputstate,

        /// <summary>
        /// 读取 LMS 的污染状态。
        /// </summary>
        LCMstate,

        /// <summary>
        /// 错误代码。
        /// </summary>
        ErrorCode,

        LMPoutputRange
    }
}