using System;

namespace Heapy.Core.Interface
{
    public interface IPrivateHeapNative
    {
        IntPtr HeapCreate(uint flOptions, IntPtr dwInitialSize, IntPtr dwMaximumSize);
        bool HeapDestroy(IntPtr hHeap);
        IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, IntPtr dwBytes);
        IntPtr HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr ptr, IntPtr dwBytes);
        bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);
    }
}