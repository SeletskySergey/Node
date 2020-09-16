using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Replicator
{
    class Program
    {
        public static int GetDigits(long n)
        {
            if (n < 10L) return 1;
            if (n < 100L) return 2;
            if (n < 1000L) return 3;
            if (n < 10000L) return 4;
            if (n < 100000L) return 5;
            if (n < 1000000L) return 6;
            if (n < 10000000L) return 7;
            if (n < 100000000L) return 8;
            if (n < 1000000000L) return 9;
            if (n < 10000000000L) return 10;
            if (n < 100000000000L) return 11;
            if (n < 1000000000000L) return 12;
            if (n < 10000000000000L) return 13;
            if (n < 100000000000000L) return 14;
            if (n < 1000000000000000L) return 15;
            if (n < 10000000000000000L) return 16;
            if (n < 100000000000000000L) return 17;
            if (n < 1000000000000000000L) return 18;
            return 19;
        }

        static void Main()
        {
            var hash = ulong.MaxValue;
            var sw = Stopwatch.StartNew();

            for (var i = 0; i <= 1000000; i++)
            {
                var code16 = Unified.NewHex16(hash);
                var dc16 = Unified.Decode16(code16);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Restart();
            for (var i = 0; i <= 1000000; i++)
            {
                var code = Unified.NewHex32(hash);
                var dc = Unified.Decode32(code);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Restart();
            for (var i = 0; i <= 1000000; i++)
            {
                var code64 = Unified.NewHex64(hash);
                var dc64 = Unified.Decode64(code64);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);



            var digits = 0;
            var set = new HashSet<ulong>();

            while (true)
            {
                if (!set.Add(hash))
                {
                    Console.WriteLine($"Collision on: {set.Count}");
                }

                var currentDigits = GetDigits(set.Count);
                if (digits != currentDigits)
                {
                    digits = currentDigits;
                    Console.WriteLine($"Digits: {digits}");
                }
            }

            var arr = File.ReadAllLines("words.txt").OrderBy(f => f).ToList();

            Console.WriteLine($"Loaded {arr.Count} items");

            var fnvs = arr.ToDictionary(f => f.HashFNV1A(), f => f);
            sw.Stop();
            Console.WriteLine($"Elapsed ms {sw.ElapsedMilliseconds}");

            for (ushort i = 2; i <= 8; i++)
            {
                Console.WriteLine($"\n\nPartitions {i} used");
                var spaces = FNV.AllocatePartitions(i);

                foreach (var fnv in fnvs)
                {
                    foreach (var space in spaces)
                    {
                        if (space.Begin <= fnv.Key && fnv.Key <= space.End)
                        {
                            space.Items.Add(fnv.Key, fnv.Value);
                        }
                    }
                }

                var totalFnvs = 0;
                foreach (var space in spaces)
                {
                    totalFnvs += space.Items.Count;
                    Console.WriteLine($"Space: begin({space.Begin}):end({space.End}) contains {space.Items.Count} items");
                }
                Console.WriteLine($"Total: {totalFnvs} source total: {fnvs.Count}");
            }
        }
    }
}
