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
        private static readonly Dictionary<Type, short> TypeToIdCache = new Dictionary<Type, short>();
        private static readonly Dictionary<short, Type> IdToTypeCache = new Dictionary<short, Type>();

        static TypeNavigator()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ProtoNavAttribute), true).Length > 0)
                {
                    var attr = type.GetCustomAttribute<ProtoNavAttribute>();
                    IdToTypeCache.Add(attr.ContractTypeId, type);
                    TypeToIdCache.Add(type, attr.ContractTypeId);
                }
            }
        }

        public static Dictionary<short, Type> GetTypesWithAttribute()
        {
            return IdToTypeCache;
        }

        
        public static short GetTypeId(this object obj)
        {
            var type = obj.GetType();
            if (TypeToIdCache.ContainsKey(type))
            {
                return TypeToIdCache[type];
            }
            else
            {
                var id = type.GetCustomAttribute<ProtoNavAttribute>().ContractTypeId;
                TypeToIdCache.Add(type, id);
                return id;
            }
        }

        public static Type GetTypeById(short id)
        {
            return IdToTypeCache[id];
        }
    }
}
