using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.Heap
{
    public sealed unsafe class Managed<TValue> : IDisposable where TValue : unmanaged
    {
        public Managed(Unmanaged<TValue> unmanagedObject)
        {
            Heap = unmanagedObject.Heap;
            Handle = new HeapObject(Heap, unmanagedObject);
        }

        internal HeapObject Handle { get; }

        public IUnmanagedHeap Heap { get; }

        public UnmanagedState State => Heap.State != UnmanagedState.Available || Handle.State != UnmanagedState.Available
                                        ? UnmanagedState.Unavailable
                                        : UnmanagedState.Available;

        public ref TValue Value
        {
            get
            {
                if (State != UnmanagedState.Available)
                {
                    throw new UnmanagedObjectUnavailable();
                }

                return ref *(TValue*)Handle.DangerousGetHandle();
            }
        }

        public void Dispose()
        {
            Handle.Dispose();
        }
    }
}