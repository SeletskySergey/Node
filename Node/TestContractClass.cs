using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Node
{
    [ProtoContract, ProtoNav(19)]
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
