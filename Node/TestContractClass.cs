using System;

namespace Node
{
    public class TestContractClass
    {
        public string One { set; get; } = string.Empty;

        public DateTime Two { set; get; } = DateTime.Now;

        public byte[] Three { set; get; } = new byte[1024];
    }
}
