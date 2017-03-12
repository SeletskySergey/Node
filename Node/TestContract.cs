using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Node
{
    [ProtoContract]
    public class TestContract
    {
        public enum Test : byte
        {
            One,
            Two,
            Three
        }

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
        public Test TestEnum { get; set; }
    }
}
