using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BinarySerialization;

namespace LMS5xxConnector.Telegram
{
    public class PacketCoLaB : IBinarySerializable
    {
        const byte STX = 0x02;

        const byte ETX = 0x03;

        // [Ignore]
        // // [FieldOrder(0)]
        // public byte STX { get; set; } = 0x2;
        // [FieldOrder(1)]
        [Ignore] public TelegramContent Content { get; set; }

        // [FieldOrder(2)]
        // [SerializeUntil((byte)0x03)]
        // public List<OtherCommand> OtherCommands { get; set; }


        // [Ignore]
        // // [FieldOrder(2)]
        // public byte ETX { get; set; } = 0x3;

        public void Serialize(Stream stream, Endianness endianness, BinarySerializationContext serializationContext)
        {
            stream.WriteByte(STX);
            _serializer.Serialize(stream, Content);
            stream.WriteByte(ETX);
        }

        public static MemoryStream  ReadStreamUntilAsync(Stream stream)
        {
            while (stream.ReadByte() != STX) ;

            int nextByte;

            MemoryStream result = new MemoryStream();

            // 读取从 0x02 开始的字节直到第一个 0x03 字节
            while ((nextByte = stream.ReadByte()) != -1 && nextByte != 0x03)
            {
                result.WriteByte((byte)nextByte);
            }

            result.Seek(0, SeekOrigin.Begin);
            return  result ;

            //缓存buffer 会 读到下一个数据开始
            // var buffer = new byte[4096];
            // var ms = new MemoryStream();
            //
            // while (true)
            // {
            //     var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            //
            //     for (int i = 0; i < bytesRead; i++)
            //     {
            //         if (buffer[i] == ETX)
            //         {
            //             ms.Seek(0, SeekOrigin.Begin);
            //             return ms;
            //         }
            //
            //         ms.WriteByte(buffer[i]);
            //     }
            // }
        }

        public void Deserialize(Stream stream, Endianness endianness, BinarySerializationContext serializationContext)
        {
            var contentStream = ReadStreamUntilAsync(stream);
            // var array = contentStream.ToArray();
            // Console.WriteLine("read buffer: " + Encoding.ASCII.GetString(array));
            // Content = _serializer.Deserialize<TelegramContent>(array);
            Content = _serializer.Deserialize<TelegramContent>(contentStream);
        }

        private readonly IBinarySerializer _serializer;

        public PacketCoLaB()
        {
            _serializer = new BinarySerializer();
            // _serializer.MemberSerializing += OnMemberSerializing;
            // _serializer.MemberSerialized += OnMemberSerialized;
            // _serializer.MemberDeserializing += OnMemberDeserializing;
            // _serializer.MemberDeserialized += OnMemberDeserialized;
        }

        private static void OnMemberSerializing(object sender, MemberSerializingEventArgs e)
        {
            Console.CursorLeft = e.Context.Depth * 4;
            Console.WriteLine("S-Start: {0} @ {1}", e.MemberName, e.Offset);
        }

        private static void OnMemberSerialized(object sender, MemberSerializedEventArgs e)
        {
            Console.CursorLeft = e.Context.Depth * 4;
            var value = e.Value ?? "null";
            Console.WriteLine("S-End: {0} ({1}) @ {2}", e.MemberName, value, e.Offset);
        }

        private static void OnMemberDeserializing(object sender, MemberSerializingEventArgs e)
        {
            Console.CursorLeft = e.Context.Depth * 4;
            Console.WriteLine("D-Start: {0} @ {1}", e.MemberName, e.Offset);
        }

        private static void OnMemberDeserialized(object sender, MemberSerializedEventArgs e)
        {
            Console.CursorLeft = e.Context.Depth * 4;
            var value = e.Value ?? "null";
            Console.WriteLine("D-End: {0} ({1}) @ {2}", e.MemberName, value, e.Offset);
        }
    }
}