using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Node
{
    [DataContract(Namespace = "1")]
    public class TestContractClass
    {
        [DataMember(Order = 1)]
        public string One { set; get; } = string.Empty;
        [DataMember(Order = 2)]
        public DateTime Two { set; get; } = DateTime.Now;
        [DataMember(Order = 3)]
        public byte[] Three { set; get; } = new byte[1024];
    }
}
