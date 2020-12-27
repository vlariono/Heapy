using System;
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
        public int I3 { get; set; }
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

                var mem = Heap.Alloc<Test>().ToManaged();

                var p = 100;
                for (int i = 0; i < 10; i++)
                {
                    mem.Add(new Test()
                    {
                        I1 = p+1,
                        I2 = p+2,
                        I3 = p+3
                    });
                    p += 100;
                }

                var c = mem.Remove(
                    new Test()
                    {
                        I1 = 301,
                        I2 = 302,
                        I3 = 303
                    });

                //watch.Start();
                //for (int i = 0; i < 100000000; i++)
                //{
                //    heap.Alloc<Test>();
                //}
                //watch.Stop();
                //var ms = watch.ElapsedMilliseconds;
                //Console.WriteLine($"Execution Time: {ms} ms");


                Console.WriteLine("Before  free");
                Console.ReadLine();
            }

            Console.WriteLine($"Heap disposed");
            Console.ReadLine(); ;
        }
    }
}