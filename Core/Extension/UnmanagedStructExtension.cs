using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Extension
{
    public static class UnmanagedStructExtension
    {
        public static Unmanaged<TValue> ToUnmanaged<TValue>(this TValue value) where TValue : unmanaged
        {
            return Heap.Current.Alloc(value);
        }
    }
}