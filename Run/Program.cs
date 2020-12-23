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

            var array = new Test[2]
            {
                new Test()
                {
                    I1 = 1000,
                    I2 = 2000
                },
                new Test()
                {
                    I1 = 4000,
                    I2 = 5000
                }
            };
            var uarray = new RefArray<Test>(array,100000000);
            uarray.Add(new Test()
            {
                I1 = 100,
                I2 = 102
            });
            uarray.Add(new Test()
            {
                I1 = 200,
                I2 = 202
            });
            var t1 = uarray[0];
            var t2 = uarray[1];

            Console.WriteLine($"Local heap is current: {localHeap.Equals(HeapManager.Current)}");
            watch.Start();
            for (int i = uarray.Count; i < uarray.Length; i++)
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