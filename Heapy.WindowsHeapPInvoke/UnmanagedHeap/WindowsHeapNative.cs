using System;
using System.Runtime.InteropServices;

namespace Heapy.WindowsHeapPInvoke.UnmanagedHeap
{
    internal static class WindowsHeapNative
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapCreate(uint flOptions, IntPtr dwInitialSize, IntPtr dwMaximumSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapDestroy(IntPtr hHeap);
        
        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, IntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern IntPtr HeapReAlloc(IntPtr hHeap, uint dwFlags, IntPtr ptr, IntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);
    }
}