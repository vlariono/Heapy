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
            Heap.SetPrivateHeapFactory(WindowsPrivateHeapIndirect.Create);
            var watch = new System.Diagnostics.Stopwatch();

            using (var heap = Heap.GetPrivateHeap())
            {

                var l = new RefList<Test>(5);
                l.Add(new Test()
                {
                    I1 = 101,
                    I2 = 102
                });
                l.Add(new Test()
                {
                    I1 = 201,
                    I2 = 202
                });
                l.Add(new Test()
                {
                    I1 = 301,
                    I2 = 302
                });
                l.Add(new Test()
                {
                    I1 = 401,
                    I2 = 402
                });
                l.Add(new Test()
                {
                    I1 = 501,
                    I2 = 502
                });
                l.Add(new Test()
                {
                    I1 = 601,
                    I2 = 602
                });
                l.Add(new Test()
                {
                    I1 = 701,
                    I2 = 702
                });
                l.Add(new Test()
                {
                    I1 = 801,
                    I2 = 802
                });

                foreach (var item in l)
                {
                    Console.WriteLine($"I1:{item.I1};I2:{item.I2}");
                }

                watch.Start();
                //for (int i = 0; i < 100000000; i++)
                //{
                //    heap.Alloc<Test>();
                //}
                //uarray.RemoveAt(1);
                //foreach (var item in uarray)
                //{
                //    Console.WriteLine($"I1:{item.I1};I2:{item.I2}");
                //}

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