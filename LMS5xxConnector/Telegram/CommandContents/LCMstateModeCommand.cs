using BinarySerialization;
using LMS5xxConnector.Enums;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LCMstateModeCommand : CommandBase
    {
        /// <summary>
        /// 污染状态
        /// 使用 <see cref="LCMStateCode"/> 枚举的序列化值。
        /// </summary>
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public LCMStateCode StateCode { get; private set; }
    }
}