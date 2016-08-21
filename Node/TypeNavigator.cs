using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Node
{
    public static class TypeNavigator
    {
        public static Dictionary<short, Type> GetTypesWithAttribute(this Assembly assembly)
        {
            var dict = new Dictionary<short, Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ProtoNavAttribute), true).Length > 0)
                {
                    var attr = type.GetCustomAttribute<ProtoNavAttribute>();
                    dict.Add(attr.ContractTypeId, type);
                }
            }

            return dict;
        }

        public static short GetTypeId(this object obj)
        {
            return obj.GetType().GetCustomAttribute<ProtoNavAttribute>().ContractTypeId;
        }
    }
}
