using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContents;

namespace LMS5xxConnector.Telegram.CommandContainers
{
    /// <summary>
    /// 用于处理读取应答 (sRA) 的命令容器。
    /// 该类继承自 <see cref="CommandContainerBase"/>，并根据不同的命令内容动态绑定到相应的命令模式类。
    /// </summary>
    public class SraCommandContainer : CommandContainerBase
    {
        /// <summary>
        /// 获取或设置命令内容。
        /// 根据不同的命令类型，<see cref="CommandConnent"/> 将绑定到不同的命令模式类。
        /// </summary>
        /// <remarks>
        /// - <see cref="UniqueIdentificationModeCommand"/> 用于处理设备唯一识别信息的读取应答 (DeviceIdent, DeviceIdentOldCode)。
        /// - <see cref="LCMstateModeCommand"/> 用于处理 LCM 状态的读取应答 (LCMstate)。
        /// - <see cref="SCdevicestateModeCommand"/> 用于处理设备状态的读取应答 (SCdevicestate)。
        /// - <see cref="LIDoutputstateModeCommand"/> 用于处理输出状态的读取应答 (LIDoutputstate)。
        /// - <see cref="LMDscandataModeCommand"/> 用于处理扫描数据的读取应答 (LMDscandata)。
        /// </remarks>
        [Subtype("Command", Commands.DeviceIdentOldCode, typeof(UniqueIdentificationModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.DeviceIdent, typeof(UniqueIdentificationModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LCMstate, typeof(LCMstateModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.SCdevicestate, typeof(SCdevicestateModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LIDoutputstate, typeof(LIDoutputstateModeCommand), BindingMode = BindingMode.OneWay)]
        [Subtype("Command", Commands.LMDscandata, typeof(LMDscandataModeCommand), BindingMode = BindingMode.OneWay)]
        public override CommandBase CommandConnent { get; set; }
    }
}