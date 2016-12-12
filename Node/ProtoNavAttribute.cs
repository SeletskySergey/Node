using System;

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
