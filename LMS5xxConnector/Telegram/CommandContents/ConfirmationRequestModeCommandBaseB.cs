using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    /// <summary>
    /// 表示CoLa B协议中的确认请求模式命令基类。
    /// 该类用于处理需要停止或启动操作的命令，比如扫描数据的开始或停止。
    /// </summary>
    public class ConfirmationRequestModeCommandBaseB : CommandBaseB
    {
        /// <summary>
        /// 获取或设置操作的停止/启动状态。
        /// </summary>
        [FieldOrder(0)]
        public StopStartB StopStart { get; set; }
    }
} 