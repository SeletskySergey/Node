using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Node
{
    public class ProtoNavAttribute : Attribute
    {
        public short ContractTypeId { set; get; }
        public ProtoNavAttribute(short contractTypeId)
        {
            ContractTypeId = contractTypeId;
        }
    }
}
