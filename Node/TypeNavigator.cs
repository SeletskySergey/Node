using System;
using System.Collections.Generic;
using System.Reflection;

namespace Node
{
    public static class TypeNavigator
    {
        private static readonly Dictionary<Type, short> typeToIdCache = new Dictionary<Type, short>();
        private static readonly Dictionary<short, Type> idToTypeCache = new Dictionary<short, Type>();

        static TypeNavigator()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ProtoNavAttribute), true).Length > 0)
                {
                    var attr = type.GetCustomAttribute<ProtoNavAttribute>();
                    idToTypeCache.Add(attr.ContractTypeId, type);
                    typeToIdCache.Add(type, attr.ContractTypeId);
                }
            }
        }

        public static Dictionary<short, Type> GetTypesWithAttribute()
        {
            return idToTypeCache;
        }

        
        public static short GetTypeId(this object obj)
        {
            var type = obj.GetType();
            if (typeToIdCache.ContainsKey(type))
            {
                return typeToIdCache[type];
            }
            else
            {
                var id = type.GetCustomAttribute<ProtoNavAttribute>().ContractTypeId;
                typeToIdCache.Add(type, id);
                return id;
            }
        }

        public static Type GetTypeById(short id)
        {
            return idToTypeCache[id];
        }
    }
}
