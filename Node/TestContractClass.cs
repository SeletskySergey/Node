using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Node
{
    public enum TestEnum : byte
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
    }
}
