using System;
using Heapy.Core.Enum;
using Heapy.Core.Heap;

namespace Heapy.Core.Interface
{
    public interface IUnmanagedHeap : IDisposable
    {
        bool IsReadonly { get; }
        UnmanagedState State { get; }
        Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged;
        Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged;
        Unmanaged<TValue> Alloc<TValue>(uint bytes) where TValue : unmanaged;
        Unmanaged<TValue> Alloc<TValue>(uint bytes, uint options) where TValue : unmanaged;
        Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes) where TValue : unmanaged;
        Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes, uint options) where TValue : unmanaged;
        bool Free(IntPtr memory);
    }
}