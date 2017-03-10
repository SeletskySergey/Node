using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using ProtoBuf;

namespace Node
{
    public class Message
    {
        private static readonly Dictionary<int, Type> types = new Dictionary<int, Type>();

        public Message(byte module, byte command, byte action)
        {
            Version = 0;
            Module = module;
            Command = command;
            Action = action;
            Data = new byte[0];
            Contract = new TestContractClass();
        }

        private const ushort HeaderSize = 12;

        public byte Version;

        public byte Module;

        public byte Command;

        public byte Action;

        public byte[] Data;

        public object Contract;

        public static Message Deserialize(byte[] data)
        {
            var msg = new Message(data[1], data[2], data[3])
            {
                Version = data[0],
            };

            var length = BitConverter.ToInt32(data, 8);

            msg.Data = new byte[length];
            Buffer.BlockCopy(data, HeaderSize, msg.Data, 0, length);

            var typeId = BitConverter.ToInt32(data, 4);
            if (!types.ContainsKey(typeId))
            {
                //Improve
                var val = Assembly.GetEntryAssembly().GetTypes().Single(f => f.Name.GetHashCode() == typeId);
                types.Add(typeId, val);
            }

            using (var ms = new MemoryStream(msg.Data))
            {
                msg.Contract = Serializer.Deserialize(typeof(TestContractClass), ms);
            }


            //var json = Encoding.UTF8.GetString(msg.Data);
            //msg.Contract = JsonConvert.DeserializeObject(json, types[typeId]);

            return msg;
        }

        /// <summary>
        /// Serialize message
        /// </summary>
        /// <returns>Serialized buffer</returns>
        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, Contract);
                Data = ms.ToArray();
            }

            //var json = JsonConvert.SerializeObject(Contract);
            //Data = Encoding.UTF8.GetBytes(json);

            var buffer = new byte[HeaderSize + Data.Length];

            Buffer.SetByte(buffer, 0, Version);
            Buffer.SetByte(buffer, 1, Module);
            Buffer.SetByte(buffer, 2, Command);
            Buffer.SetByte(buffer, 3, Action);

            var hash = BitConverter.GetBytes(Contract.GetType().Name.GetHashCode());

            Buffer.BlockCopy(hash, 0, buffer, 4, hash.Length);

            var length = BitConverter.GetBytes(Data.Length);

            Buffer.BlockCopy(length, 0, buffer, 8, length.Length);

            Buffer.BlockCopy(Data, 0, buffer, 12, Data.Length);

            return buffer;
        }

        /// <summary>
        /// Get full message buffer from stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>Message buffer</returns>
        public static byte[] Get(Stream stream)
        {
            var header = new byte[HeaderSize];

            var count = stream.Read(header, 0, header.Length);
            while (count < HeaderSize)
            {
                count += stream.Read(header, count, HeaderSize - count);
            }

            var length = BitConverter.ToInt32(header, 8);

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
    }
}