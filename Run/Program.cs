using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heapy.Core.Extension;
using Heapy.Core.UnmanagedHeap;
using Heapy.WindowsHeapIndirect.UnmanagedHeap;
using Heapy.WindowsHeapPInvoke.UnmanagedHeap;

namespace Run
{
    internal struct Test
    {
        public int I1 { get; set; }
        public int I2 { get; set; }
        //public int I3 { get; set; }
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Array");
            var b = new Test[100000000];
            watch.Start();
            for (int i = 0; i < 100000000; i++)
            {
                b[i] = new Test();
            }
            watch.Stop();
            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
            b = null;

            watch.Reset();
            Console.WriteLine("List");
            var l = new List<Test>();
            watch.Start();
            for (int i = 0; i < 100000000; i++)
            {
                l.Add(new Test());
            }
            watch.Stop();
            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            watch.Reset();
            Console.WriteLine("Unmanaged");
            using (var heap = WindowsPrivateHeapIndirect.Create())
            {
                using (var unm = heap.Alloc<Test>(100000000))
                {
                    watch.Start();
                    for (int i = 0; i < 100000000; i++)
                    {
                        unm[i] = new Test();
                    }
                    watch.Stop();
                    Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
                }
            }

            watch.Reset();
            Console.WriteLine("Expandable");
            using (var heap = WindowsPrivateHeapIndirect.Create())
            {
                using var exp = heap.AllocExpandable<Test>();
                watch.Start();
                for (int i = 0; i < 100000000; i++)
                {
                    exp.Add(new Test());
                }
                watch.Stop();
                Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
            }

            Console.WriteLine($"Heap disposed");
        }
    }
}