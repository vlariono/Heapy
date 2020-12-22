using Heapy.Core.Heap;

namespace Heapy.Core.Extension
{
    public static class UnmanagedStructExtension
    {
        public static Unmanaged<TValue> ToUnmanaged<TValue>(this TValue value) where TValue : unmanaged
        {
            return HeapManager.Current.Alloc(value);
        }
    }
}