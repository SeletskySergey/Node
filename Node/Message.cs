using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

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

            var json = Encoding.Default.GetString(msg.Data);
            msg.Contract = JsonConvert.DeserializeObject(json, types[typeId]);

            return msg;
        }

        public byte[] Serialize()
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

                    ms.Write(BitConverter.GetBytes(Contract.GetType().Name.GetHashCode()), 0, 4);

                    var json = JsonConvert.SerializeObject(Contract);
                    Data = Encoding.Default.GetBytes(json);

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