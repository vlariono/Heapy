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

    internal unsafe class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var watch = new System.Diagnostics.Stopwatch();
            HeapManager.Init(WindowsPrivateHeapIndirect.Create);
            //var localHeap = HeapManager.SetLocalHeap(new ProcessHeap());

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
            uarray.Add(new Test()
            {
                I1 = 300,
                I2 = 302
            });
            uarray.Add(new Test()
            {
                I1 = 400,
                I2 = 402
            });

            Console.WriteLine($"Count before removal {uarray.Count}");
            uarray.RemoveAt(3);
            Console.WriteLine($"Count after removal {uarray.Count}");
            watch.Start();
            for (int i = 0; i < uarray.Count; i++)
            {
                Console.WriteLine($"I1: {uarray[i].I1} I2: {uarray[i].I2}");
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
            //Console.WriteLine($"{localHeap.State}");
            Console.ReadLine();
        }
    }
}