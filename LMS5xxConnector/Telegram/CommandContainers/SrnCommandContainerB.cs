using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    /// <summary>
    /// 表示CoLa B协议中的SRN命令容器。
    /// 用于封装读取操作的命令和响应。
    /// </summary>
    public class SrnCommandContainerB : CommandContainerBaseB
    {
        [Subtype("Command", Commands.DeviceIdentOldCode, typeof(UniqueIdentificationModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.DeviceIdent, typeof(UniqueIdentificationModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LCMstate, typeof(LCMstateModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.SCdevicestate, typeof(SCdevicestateModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LIDoutputstate, typeof(LIDoutputstateModeCommandB), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LMDscandata, typeof(LMDscandataModeCommandB), BindingMode = BindingMode.OneWay)]
        public override CommandBaseB CommandConnent { get; set; }
    }
} 