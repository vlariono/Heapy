using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Heapy.Core.Collection;
using Heapy.Core.UnmanagedHeap;
using Heapy.WindowsHeapIndirect.UnmanagedHeap;
using Heapy.WindowsHeapPInvoke.UnmanagedHeap;

namespace Run
{
    internal struct Test
    {
        public int I1 { get; set; }
        public int I2 { get; set; }
    }

    internal unsafe class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Heap.SetPrivateHeapFactory(WindowsPrivateHeapPInvoke.Create);
            var watch = new System.Diagnostics.Stopwatch();

            using (Heap.GetPrivateHeap())
            {
                var uarray = new RefArray<Test>(100000000);
                watch.Start();
                for (int i = 0; i < 100000000; i++)
                {
                    Heap.Alloc<Test>();
                }

                watch.Stop();
                var ms = watch.ElapsedMilliseconds;
                Console.WriteLine($"Execution Time: {ms} ms");
                Console.WriteLine("Before  free");
                Console.ReadLine();
            }

            Console.WriteLine($"Heap disposed");
            Console.ReadLine(); ;
        }
    }
}