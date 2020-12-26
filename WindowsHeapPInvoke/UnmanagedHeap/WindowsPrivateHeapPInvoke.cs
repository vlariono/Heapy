using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.WindowsHeap.Interface;
using Heapy.WindowsHeap.UnmanagedHeap;

namespace Heapy.WindowsHeapPInvoke.UnmanagedHeap
{
    public sealed class WindowsPrivateHeapPInvoke:IWindowsPrivateHeapNative
    {
        private static readonly IWindowsPrivateHeapNative Kernel32Lib;

        private WindowsPrivateHeapPInvoke() { }
        
        static WindowsPrivateHeapPInvoke()
        {
            Kernel32Lib = new WindowsPrivateHeapPInvoke();
        }
        
        public static IUnmanagedHeap Create()
        {
            return Create(4194304, 0);
        }

        public static IUnmanagedHeap Create(uint initialSize, uint maximumSize, uint options = (uint)WindowsHeapOptions.Default)
        {
            var heapHandle = Kernel32Lib.HeapCreate(options, (IntPtr)initialSize, (IntPtr)maximumSize);
            if (heapHandle == IntPtr.Zero)
            {
                throw new UnmanagedHeapUnavailable("Failed to create heap");
            }

            return new WindowsPrivateHeap(heapHandle,Kernel32Lib);
        }
        
        public IntPtr HeapCreate(uint flOptions, IntPtr dwInitialSize, IntPtr dwMaximumSize)
        {
            return WindowsHeapNative.HeapCreate(flOptions, dwInitialSize, dwMaximumSize);
        }

        public bool HeapDestroy(IntPtr hHeap)
        {
            return WindowsHeapNative.HeapDestroy(hHeap);
        }

        public IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, IntPtr dwBytes)
        {
            return WindowsHeapNative.HeapAlloc(hHeap, dwFlags, dwBytes);
        }

        public IntPtr HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr ptr, IntPtr dwBytes)
        {
            return WindowsHeapNative.HeapReAlloc(hHeap, dwFlags, ptr, dwBytes);
        }

        public bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem)
        {
            return WindowsHeapNative.HeapFree(hHeap, dwFlags, lpMem);
        }
    }
}