using System;
using System.Runtime.InteropServices;
using Heapy.Core.Enum;
using Heapy.Core.Interface;

namespace Heapy.Core.Heap
{
    internal class HeapObject : SafeHandle
    {
        public HeapObject(IUnmanagedHeap heap, IntPtr handle) : base(IntPtr.Zero, true)
        {
            Heap = heap;
            SetHandle(handle);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        public UnmanagedState State => Heap.State != UnmanagedState.Available || IsClosed || IsInvalid
            ? UnmanagedState.Unavailable
            : UnmanagedState.Available;

        public IUnmanagedHeap Heap { get; }

        protected override bool ReleaseHandle()
        {
            return Heap.Free(handle);
        }
    }
}