using BinarySerialization;

namespace LMS5xxConnector
{
    public enum Commands
    {
        SetAccessMode,

        [SerializeAsEnum("mLMPsetscancfg")]
        SetScanConfig,

        [SerializeAsEnum("LMDscandatacfg")]
        ScanDataConfig,

        // 获取设备信息
        [SerializeAsEnum("0")]
        DeviceIdentOldCode,
        //获取设备信息
        DeviceIdent,//DeviceIdent=="0"
        //设置时间搓
        LSPsetdatetime,
        Run,
        LMDscandata,
        //设置输出状态
        mDOSetOutput,
        //重置输出计数器
        LIDrstoutpcnt,
        //重启设备
        mSCreboot,
        //读取设备状态
        SCdevicestate,
        //每次输出状态改变时发送输出报文
        LIDoutputstate,
        //读取 LMS 的污染状态
        LCMstate,
        //之定义类型

        ErrorCode
    }
}