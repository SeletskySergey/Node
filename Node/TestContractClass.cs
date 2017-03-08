using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Node
{
    public enum TestEnum
    {
        One,
        Two,
        Three
    }

    [ProtoContract]
    public class TestContractClass
    {
        [ProtoMember(1)]
        public string One { set; get; }

        [ProtoMember(2)]
        public DateTime Two { set; get; }

        [ProtoMember(3)]
        public byte[] Three { set; get; }

        [ProtoMember(4)]
        public long Count { set; get; }

        [ProtoMember(5)]
        public bool Active { get; set; }

        [ProtoMember(6)]
        public List<string> Strings { get; set; } = new List<string>();

        [ProtoMember(7)]
        public TestEnum TestEnum { get; set; }

        public byte[] Serialize()
        {
            var lengthSize = 2;

            var one = Encoding.UTF8.GetBytes(One);

            var two = BitConverter.GetBytes(Two.Ticks);

            var count = BitConverter.GetBytes(Count);

            var active = BitConverter.GetBytes(Active);

            var strList = Strings.Aggregate((i, j) => i + "|" + j);
            var listBytes = Encoding.UTF8.GetBytes(strList);

            var oneSize = BitConverter.GetBytes((ushort)one.Length);
            var threeSize = BitConverter.GetBytes((ushort)Three.Length);
            var listBytesSize = BitConverter.GetBytes((ushort)listBytes.Length);

            var buffer = new byte[lengthSize + one.Length + two.Length + threeSize.Length + Three.Length + count.Length + active.Length + listBytesSize.Length + listBytes.Length + 2];

            Buffer.BlockCopy(oneSize, 0, buffer, 0, lengthSize);
            Buffer.BlockCopy(one, 0, buffer, lengthSize, one.Length);
            Buffer.BlockCopy(two, 0, buffer, lengthSize + one.Length, two.Length);
            Buffer.BlockCopy(threeSize, 0, buffer, lengthSize + one.Length + two.Length, threeSize.Length);
            Buffer.BlockCopy(Three, 0, buffer, lengthSize + one.Length + two.Length + threeSize.Length, Three.Length);

            Buffer.BlockCopy(count, 0, buffer, lengthSize + one.Length + two.Length + threeSize.Length + Three.Length, count.Length);

            Buffer.BlockCopy(active, 0, buffer, lengthSize + one.Length + two.Length + threeSize.Length + Three.Length + count.Length, active.Length);

            Buffer.BlockCopy(listBytesSize, 0, buffer, lengthSize + one.Length + two.Length + threeSize.Length + Three.Length + count.Length + active.Length, listBytesSize.Length);

            Buffer.BlockCopy(listBytes, 0, buffer, lengthSize + one.Length + two.Length + threeSize.Length + Three.Length + count.Length + active.Length + listBytesSize.Length, listBytes.Length);

            Buffer.SetByte(buffer, lengthSize + one.Length + two.Length + threeSize.Length + Three.Length + count.Length + active.Length + listBytesSize.Length + listBytes.Length + 1, (byte)TestEnum);

            return buffer;
        }

        public static TestContractClass Deserialize(byte[] bytes)
        {
            var lengthSize = 2;

            var contract = new TestContractClass();

            var oneSize = BitConverter.ToInt16(bytes, 0);
            var oneBytes = new byte[oneSize];
            Buffer.BlockCopy(bytes, lengthSize, oneBytes, 0, oneSize);
            contract.One = Encoding.UTF8.GetString(oneBytes);

            var two = BitConverter.ToInt64(bytes, lengthSize + oneSize);
            contract.Two = new DateTime(two);

            var threeSize = BitConverter.ToInt16(bytes, lengthSize + oneSize + 8);

            contract.Three = new byte[threeSize];

            Buffer.BlockCopy(bytes, lengthSize + oneSize + 8 + lengthSize, contract.Three, 0, threeSize);

            contract.Count = BitConverter.ToInt64(bytes, lengthSize + oneSize + 8 + lengthSize + threeSize);
            contract.Active = BitConverter.ToBoolean(bytes, lengthSize + oneSize + 8 + lengthSize + threeSize + 8);

            var listSize = BitConverter.ToInt16(bytes, lengthSize + oneSize + 8 + lengthSize + threeSize + 8 + 1);
            var listBytes = new byte[listSize];

            Buffer.BlockCopy(bytes, lengthSize + oneSize + 8 + lengthSize + threeSize + 8 + 1 + lengthSize, listBytes, 0, listSize);

            contract.Strings = Encoding.UTF8.GetString(listBytes).Split('|').ToList();

            contract.TestEnum = (TestEnum)Buffer.GetByte(bytes, lengthSize + oneSize + 8 + lengthSize + threeSize + 8 + 1 + lengthSize + listSize + 1);

            return contract;
        }
    }
}
