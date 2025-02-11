using BinarySerialization;
using LMS5xxConnector.Telegram.CommandContainers;

namespace LMS5xxConnector.Telegram
{

    /// <summary>
    /// 表示 CoLa A 通讯协议中的报文内容。
    /// 该类用于封装命令类型及其对应的负载（Payload），并支持不同类型命令的序列化和反序列化。
    /// </summary>
    public class TelegramContent
    {
        /// <summary>
        /// 获取或设置命令类型。
        /// 命令类型决定了报文的具体功能，例如读取、写入、执行方法等。
        /// 该字段在序列化时以终止字符串的形式表示，并以空格字符 (0x20) 作为终止符。
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public CommandTypes CommandTypes { get; set; }


        /// <summary>
        /// 获取或设置命令的负载（Payload）。
        /// 根据不同的命令类型，负载可以是不同类型的对象，具体取决于命令的功能。
        /// 通过使用 <see cref="SubtypeAttribute"/>，可以根据 <see cref="CommandTypes"/> 的值动态绑定到不同的命令容器类。
        /// </summary>
        /// <remarks>
        /// - <see cref="SrnCommandContainer"/> 用于处理读取命令 (sRN)。
        /// - <see cref="SwnCommandContainer"/> 用于处理写入命令 (sWN)。
        /// - <see cref="SmnCommandContainer"/> 用于处理方法命令 (sMN)。
        /// - <see cref="SenCommandContainer"/> 用于处理事件命令 (sEN) 和事件应答 (sEA)。
        /// - <see cref="SraCommandContainer"/> 用于处理读取应答 (sRA)。
        /// - <see cref="SwaCommandContainer"/> 用于处理写入应答 (sWA)。
        /// - <see cref="SeaCommandContainer"/> 用于处理事件应答 (sEA)。
        /// - <see cref="SsnCommandContainer"/> 用于处理状态查询 (sSN)。
        /// - <see cref="SanCommandContainer"/> 用于处理通用应答 (sAN)。
        /// - <see cref="SfaCommandContainer"/> 用于处理错误代码 (sFA)。
        /// </remarks>
        [FieldOrder(1)]
        [Subtype("CommandTypes", CommandTypes.Srn, typeof(SrnCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Swn, typeof(SwnCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Smn, typeof(SmnCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sen, typeof(SenCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sra, typeof(SraCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Swa, typeof(SenCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sea, typeof(SenCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Ssn, typeof(SsnCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.San, typeof(SanCommandContainer), BindingMode = BindingMode.OneWay)]
        [Subtype("CommandTypes", CommandTypes.Sfa, typeof(SfaCommandContainer), BindingMode = BindingMode.OneWay)]
        public CommandContainerBase Payload { get; set; }
    }

}