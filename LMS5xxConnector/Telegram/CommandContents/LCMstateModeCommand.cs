using BinarySerialization;

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


    
    /// <summary>
    /// 污染状态。
    /// </summary>
    public enum LCMStateCode
    {
        /// <summary>
        /// 无污染。
        /// 设备处于正常工作状态，没有检测到污染。
        /// </summary>
        [SerializeAsEnum("0")]
        NoContamination = 0,

        /// <summary>
        /// 污染警告。
        /// 设备检测到轻微污染，建议进行清洁。
        /// </summary>
        [SerializeAsEnum("1")]
        ContaminationWarning = 1,

        /// <summary>
        /// 污染错误。
        /// 设备检测到严重污染，可能需要立即清洁或维护。
        /// </summary>
        [SerializeAsEnum("2")]
        ContaminationError = 2,

        /// <summary>
        /// 测量功能缺陷。
        /// 设备的测量功能受到污染影响，无法正常工作。
        /// </summary>
        [SerializeAsEnum("3")]
        MeasurementFunctionalityDefective = 3
    }
}