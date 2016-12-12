using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replicator
{
    class Program
    {
        static void Main(string[] args)
        {
            var arr = File.ReadAllLines("words.txt").OrderBy(f => f).ToList();

            Console.WriteLine($"Loaded {arr.Count} items");

            var sw = Stopwatch.StartNew();
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
