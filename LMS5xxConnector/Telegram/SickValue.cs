using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinarySerialization;

namespace LMS5xxConnector.Telegram
{
    public class SickValue<T> : IFormattable, IBinarySerializable where T : struct, IConvertible,
        IFormattable,
        IComparable<T>,
        IEquatable<T>
    {
        public T Value { get; set; }

        public void Serialize(Stream stream, Endianness endianness, BinarySerializationContext serializationContext)
        {
            string hexStr;
            if (Value is float v)
            {
                var bytes = BitConverter.GetBytes(v);
                hexStr = Convert.ToString(BitConverter.ToInt32(bytes, 0), 16);
            }
            else
            {
                hexStr = Convert.ToString(Value.ToInt64(null), 16);
            }

            stream.Write(Encoding.UTF8.GetBytes(hexStr));
            stream.WriteByte(0x20);
        }

        static readonly Dictionary<Type, Func<string, T>> TypeConverters = new Dictionary<Type, Func<string, T>>
        {
            { typeof(int), value => (T)(object)Convert.ToInt32(value, 16) },
            { typeof(uint), value => (T)(object)Convert.ToUInt32(value, 16) },
            { typeof(long), value => (T)(object)Convert.ToInt64(value, 16) },
            { typeof(ushort), value => (T)(object)Convert.ToUInt16(value, 16) },
            {
                typeof(float),
                value => (T)(object)BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(value, 16)), 0)
            },
        };

        public void Deserialize(Stream stream, Endianness endianness, BinarySerializationContext serializationContext)
        {
            var hexString = ReadStringUntilSpace(stream);

            if (TypeConverters.TryGetValue(typeof(T), out var converter))
            {
                Value = converter(hexString);
            }
            else
            {
                throw new NotImplementedException("Not Implemented SickValue type:" + typeof(T));
            }
        }

        public override string ToString() => Value.ToString();

        // public string ToString(string? format) => _value.ToString(format);

        public static string ReadStringUntilSpace(Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            int value;
            while ((value = stream.ReadByte()) != -1 && value != 0x20)
            {
                sb.Append((char)value);
            }

            return sb.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }
    }
}