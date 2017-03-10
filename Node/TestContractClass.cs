using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Node
{
    public class Builder<T>
    {
        public Builder()
        {
            Index = 0;
        }

        public int Index { set; get; }

        public List<Action<T, byte[]>> Retrivers { set; get; } = new List<Action<T, byte[]>>();

        public List<Action<T, List<byte[]>>> Builds { set; get; } = new List<Action<T, List<byte[]>>>();
    }

    public static class ManualSerializer
    {
        const int lengthSize = 2;

        static Action<T, List<byte[]>> BuildAddAccessor<T>(Expression<Func<Action<T, List<byte[]>>>> method)
        {
            return method.Compile()();
        }

        static Action<T, byte[]> BuildRetriverAccessor<T>(Expression<Func<Action<T, byte[]>>> method)
        {
            return method.Compile()();
        }

        static Action<T, object> BuildSetAccessor<T>(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(T), "o");
            var value = Expression.Parameter(typeof(object));

            var expr = Expression.Lambda<Action<T, object>>(
                    Expression.Call(
                        Expression.Convert(obj, method.DeclaringType),
                        method,
                        Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                    obj,
                    value);

            return expr.Compile();
        }

        static Func<T, object> BuildGetAccessor<T>(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(T), "o");

            var expr = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(
                        Expression.Call(
                            Expression.Convert(obj, method.DeclaringType),
                            method),
                        typeof(object)),
                    obj);

            return expr.Compile();
        }

        public static Builder<TSource> Build<TSource>(this TSource source)
        {
            var builder = new Builder<TSource>();
            var properties = source.GetType().GetProperties();

            foreach (var property in properties)
            {
                var setter = BuildSetAccessor<TSource>(property.SetMethod);
                var getter = BuildGetAccessor<TSource>(property.GetMethod);
                builder.Add(property.PropertyType, getter);
                builder.Retrive(property.PropertyType, setter);
            }

            return builder;
        }

        public static Builder<TSource> Retrive<TSource>(this Builder<TSource> source, Type type, Action<TSource, object> setter)
        {
            Action<TSource, byte[]> method = null;
            if (type == typeof(string))
            {
                method = (item, buffer) =>
                {
                    var length = BitConverter.ToInt16(buffer, source.Index);
                    source.Index += lengthSize;

                    var data = new byte[length];

                    Buffer.BlockCopy(buffer, source.Index, data, 0, length);

                    source.Index += length;

                    var text = Encoding.UTF8.GetString(data);

                    setter(item, text);
                };
            }
            else if (type == typeof(byte[]))
            {
                method = (item, buffer) => {
                    var length = BitConverter.ToInt16(buffer, source.Index);
                    source.Index += lengthSize;
                    var data = new byte[length];

                    Buffer.BlockCopy(buffer, source.Index, data, 0, length);

                    source.Index += length;

                    setter(item, data);
                };
            }
            else if (type == typeof(int))
            {
                method = (item, buffer) => {
                    var number = BitConverter.ToInt32(buffer, source.Index);
                    source.Index += sizeof(int);
                    setter(item, number);
                }; 
            }
            else if (type == typeof(long))
            {
                method = (item, buffer) => {
                    var number = BitConverter.ToInt64(buffer, source.Index);
                    source.Index += sizeof(long);
                    setter(item, number);
                };
            }
            else if (type == typeof(bool))
            {
                method = (item, buffer) => {
                    var boolean = BitConverter.ToBoolean(buffer, source.Index);
                    source.Index += sizeof(bool);
                    setter(item, boolean);
                };  
            }
            else if (type == typeof(DateTime))
            {
                method = (item, buffer) => {
                    var ticks = BitConverter.ToInt64(buffer, source.Index);
                    source.Index += sizeof(long);
                    setter(item, new DateTime(ticks));
                };
            }
            else if (type == typeof(List<string>))
            {
                method = (item, buffer) => {
                    var length = BitConverter.ToInt16(buffer, source.Index);
                    source.Index += lengthSize;
                    var data = new byte[length];

                    Buffer.BlockCopy(buffer, source.Index, data, 0, length);

                    source.Index += length;

                    var list = Encoding.UTF8.GetString(data).Split('|').ToList();

                    setter(item, list);
                };
                
            }
            else if (type.IsEnum)
            {
                method = (item, buffer) => {
                    var value = Buffer.GetByte(buffer, source.Index);
                    source.Index += sizeof(byte);
                    setter(item, value);
                }; 
            }

            var retriver = BuildRetriverAccessor(() => method);
            source.Retrivers.Add(retriver);

            return source;
        }

        public static Builder<TSource> Add<TSource>(this Builder<TSource> source, Type type, Func<TSource, object> getter)
        {
            Action<TSource, List<byte[]>> method = null;

            if (type == typeof(string))
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    var text = (string)item;
                    var bytes = Encoding.UTF8.GetBytes(text);
                    var length = BitConverter.GetBytes((ushort)bytes.Length);

                    blocks.Add(length);
                    blocks.Add(bytes);
                };  
            }
            else if (type == typeof(byte[]))
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    var bytes = (byte[])item;
                    var length = BitConverter.GetBytes((ushort)bytes.Length);

                    blocks.Add(length);
                    blocks.Add(bytes);
                }; 
            }
            else if (type == typeof(int))
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    var bytes = BitConverter.GetBytes((int)item);
                    blocks.Add(bytes);
                };  
            }
            else if (type == typeof(long))
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    var bytes = BitConverter.GetBytes((long)item);
                    blocks.Add(bytes);
                };
            }
            else if (type == typeof(bool))
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    var bytes = BitConverter.GetBytes((bool)item);
                    blocks.Add(bytes);
                };
            }
            else if(type == typeof(DateTime))
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    var dateTime = (DateTime)item;
                    var bytes = BitConverter.GetBytes(dateTime.Ticks);
                    blocks.Add(bytes);
                };
            }
            else if (type == typeof(List<string>))
            {
                method = (src, blocks) => {
                    var item = getter(src);

                    var list = (List<string>)item;

                    var text = list.Aggregate((i, j) => i + "|" + j);

                    var bytes = Encoding.UTF8.GetBytes(text);
                    var length = BitConverter.GetBytes((ushort)bytes.Length);

                    blocks.Add(length);
                    blocks.Add(bytes);
                };
            }
            else if (type.IsEnum)
            {
                method = (src, blocks) => {
                    var item = getter(src);
                    blocks.Add(new byte[1] { (byte)item });
                };
            }

            var builder = BuildAddAccessor(() => method);
            source.Builds.Add(builder);

            return source;
        }

        public static byte[] Serialize<TSource>(this Builder<TSource> source, TSource instance)
        {
            var blocks = new List<byte[]>();

            foreach (var action in source.Builds)
            {
                action(instance, blocks);
            }

            var length = blocks.Select(f => f.Length).Sum();
            var buffer = new byte[length];

            var offset = 0;
            foreach(var item in blocks)
            {
                Buffer.BlockCopy(item, 0, buffer, offset, item.Length);
                offset += item.Length;
            }

            return buffer;
        }

        public static TSource Deserialize<TSource>(this Builder<TSource> source, byte[] data)
        {
            var item = Activator.CreateInstance<TSource>();
            source.Index = 0;
            foreach (var action in source.Retrivers)
            {
                action(item, data);
            }
            return item;
        }
    }

    public enum TestEnum : byte
    {
        One,
        Two,
        Three
    }

    [ProtoContract]
    public class TestContractClass
    {
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
        public TestEnum TestEnum { get; set; }
    }
}
