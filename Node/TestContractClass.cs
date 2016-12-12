using System;
using ProtoBuf;

namespace Node
{
    [ProtoContract]
    public class TestContractClass
    {
        [ProtoMember(1)]
        public string One { set; get; } = string.Empty;
        [ProtoMember(2)]
        public DateTime Two { set; get; } = DateTime.Now;
        [ProtoMember(3)]
        public byte[] Three { set; get; } = new byte[1024];
    }
}
