﻿using System;
using System.IO;
using System.IO.Compression;
using ProtoBuf;

namespace Node
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
            Contract = new TestContractClass();
        }

        private const ushort HeaderSize = 10;

        public byte Version;

        public byte Module;

        public byte Command;

        public byte Action;

        public byte[] Data;

        public object Contract;

        public static Message Deserialize(byte[] data, bool decompress = false)
        {
            var msg = new Message(data[1], data[2], data[3])
            {
                Version = data[0],
            };

            var length = BitConverter.ToInt32(data, 6);
            var body = new byte[length];
            Buffer.BlockCopy(data, HeaderSize, body, 0, length);

            if (decompress)
            {
                msg.Data = Decompress(body);
            }

            var typeId = BitConverter.ToInt16(data, 4);
            using (var ms = new MemoryStream(msg.Data))
            {
                msg.Contract = Serializer.Deserialize(TypeNavigator.GetTypeById(typeId), ms);
            }

            return msg;
        }

        public byte[] Serialize(bool commpress = false)
        {
            var bytes = new byte[0];
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.WriteByte(Version);
                    ms.WriteByte(Module);
                    ms.WriteByte(Command);
                    ms.WriteByte(Action);

                    ms.Write(BitConverter.GetBytes(Contract.GetTypeId()), 0, 2);
                    using (var output = new MemoryStream())
                    {
                        Serializer.Serialize(output, Contract);
                        Data = output.ToArray();
                    }

                    if (commpress)
                    {
                        Data = Compress(Data);
                    }

                    ms.Write(BitConverter.GetBytes(Data.Length), 0, 4);
                    ms.Write(Data, 0, Data.Length);
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

            var length = BitConverter.ToInt32(header, 6);

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
                    using (var output = new MemoryStream())
                    {
                        input.CopyTo(output);
                        return output.ToArray();
                    }    
                }
            }
        }
    }
}