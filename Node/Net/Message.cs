using System;
using System.IO;
using System.IO.Compression;

namespace Node.Net
{
    public struct Message
    {
        public Message(byte module, byte command, byte action)
        {
            this = new Message();
            Version = 0;
            Module = module;
            Command = command;
            Action = action;
            Data = new byte[0];
        }

        private const ushort HeaderSize = 8;

        public byte Version;

        public byte Module;

        public byte Command;

        public byte Action;

        public byte[] Data;

        public static Message Deserialize(byte[] data)
        {
            var length = BitConverter.ToInt32(data, 4);
            var body = new byte[length];
            Buffer.BlockCopy(data, HeaderSize, body, 0, length);

            var msg = new Message(data[1], data[2], data[3])
            {
                Version = data[0],
                Data = Decompress(body)
            };
            return msg;
        }

        public static byte[] Serialize(Message msg)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                ms.WriteByte(msg.Version);
                ms.WriteByte(msg.Module);
                ms.WriteByte(msg.Command);
                ms.WriteByte(msg.Action);

                msg.Data = Compress(msg.Data);

                ms.Write(BitConverter.GetBytes(msg.Data.Length), 0, 4);
                ms.Write(msg.Data, 0, msg.Data.Length);
                bytes = ms.ToArray();
            }
            return bytes;
        }

        public static byte[] Get(Stream stream)
        {
            var header = new byte[HeaderSize];

            var count = stream.Read(header, 0, header.Length);
            while (count < HeaderSize)
            {
                count += stream.Read(header, count, HeaderSize - count);
            }

            var length = BitConverter.ToInt32(header, 4);

            var data = new byte[HeaderSize + length];

            Buffer.BlockCopy(header, 0, data, 0, HeaderSize);
            if (length > 0)
            {
                count = stream.Read(data, HeaderSize, length);
                while (count < length)
                {
                    count += stream.Read(data, HeaderSize + count, length - count);
                }
            }
            return data;
        }

        private static byte[] Compress(byte[] bytes)
        {
            using (var destination = new MemoryStream())
            {
                using (var output = new GZipStream(destination, CompressionMode.Compress))
                {
                    output.Write(bytes, 0, bytes.Length);
                }
                return destination.ToArray();
            }
        }

        private static byte[] Decompress(byte[] bytes)
        {
            using (var source = new MemoryStream(bytes))
            {
                using (var input = new GZipStream(source, CompressionMode.Decompress))
                {
                    var buffer = new byte[input.Length];
                    var count = input.Read(buffer, 0, (int)input.Length);
                    var destination = new byte[count];
                    Buffer.BlockCopy(buffer, 0, destination, 0, count);
                    return destination;
                }
            }
        }
    }
}