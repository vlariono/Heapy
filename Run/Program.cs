using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WindowsHeapIndirect.Heap;
using Heapy.Core.Collection;
using Heapy.Core.Heap;
using Heapy.GeneralHeap.Heap;
using Heapy.WindowsHeapPInvoke.Heap;

namespace Run
{
    internal struct Test
    {
        public int I1 { get; set; }
        public int I2 { get; set; }
    }

    [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
    internal  unsafe ref struct Test2
    {
        public int I1 { get; set; }
        public byte* Str { get; set; }
    }

    internal class CTest
    {
        public int I1 { get; set; }
        public int I2 { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var watch = new System.Diagnostics.Stopwatch();

            HeapManager.Init(WindowsPrivateHeapIndirect.Create);
            var localHeap = HeapManager.SetLocalHeap(new ProcessHeap());
            
            var list = new List<Test>();
            var uarray = new RefArray<Test>(100000000);
            
            Console.WriteLine($"Local heap is current: {localHeap.Equals(HeapManager.Current)}");
            watch.Start();
            for (int i = 0; i < 100000000; i++)
            {
                uarray.Add(new Test());

            }
            watch.Stop();
            var ms = watch.ElapsedMilliseconds;
            Console.WriteLine($"Execution Time: {ms} ms");
            Console.WriteLine("Before  free");
            Console.ReadLine();
            uarray.Dispose();
            //list3 = null;
            //GC.Collect();
            //Console.WriteLine($"{localHeap.State}");
            //Console.WriteLine("Dispose last object");
            //Console.ReadLine();
            Console.WriteLine($"{localHeap.State}");
            Console.ReadLine();
        }
    }
}