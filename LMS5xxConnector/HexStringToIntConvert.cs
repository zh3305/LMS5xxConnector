using BinarySerialization;

namespace LMS5xxConnector
{
    class HexStringToIntConvert : IValueConverter
    {
        // Read
        public object Convert(object value, object parameter, BinarySerializationContext context)
        {
            return System.Convert.ToInt32((string)value, 16);
        }

        //Write
        public object ConvertBack(object value, object parameter, BinarySerializationContext context)
        {
            return value switch
            {
                int intValue => intValue.ToString("X"),
                long longValue => longValue.ToString("X"),
                _ => value
            };
        }
    }
}