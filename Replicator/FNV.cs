using System.Collections.Generic;
using System.Text;

namespace Replicator
{
    public static class FNV
    {
        private const ulong FNV64Offset = 14695981039346656037;

        private const ulong FNV64Prime = 1099511628211;

        private const ulong FNV64Space = ulong.MaxValue - FNV64Prime;

        public static ulong HashFNV1A(this string key)
        {
            var bytes = Encoding.Default.GetBytes(key.ToUpperInvariant());
            var hash = FNV64Offset;
            foreach (var @byte in bytes)
            {
                hash ^= @byte;
                hash *= FNV64Prime;
            }
            return hash;
        }

        public class Partition
        {
            public int Id;
            public Dictionary<ulong, string> Items = new Dictionary<ulong, string>();
            public Partition(ulong begin, ulong end)
            {
                Begin = begin;
                End = end;
            }

            public ulong Begin;
            public ulong End;
        }

        public static List<Partition> AllocatePartitions(ushort count)
        {
            var part = FNV64Space / count;
            var shift = FNV64Space % count;
            var partitions = new List<Partition>();

            var index = 1;
            for (var begin = FNV64Prime; begin < ulong.MaxValue - shift; begin += part)
            {
                index++;
                if (begin + part >= ulong.MaxValue - shift)
                {
                    part += shift;
                }
                partitions.Add(new Partition(begin, begin + part) { Id = index });
            }
            return partitions;
        }
    }
}
