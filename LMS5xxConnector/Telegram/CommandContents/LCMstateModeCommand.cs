using BinarySerialization;

namespace LMS5xxConnector.Telegram.CommandContents
{
    public class LCMstateModeCommand : CommandBase
    {
        //没有污染。0
        //污染警告：1
        //污染错误：2
        //污染测量功能缺陷：3
        [FieldOrder(0)]
        [SerializeAs(SerializedType.TerminatedString, StringTerminator = (char)0x20)]
        public string StatusCode { get; set; }
    }
}