using System;
using System.IO;
using AdvancedDLSupport;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.WindowsHeap.Interface;
using Heapy.WindowsHeap.UnmanagedHeap;

namespace Heapy.WindowsHeapIndirect.UnmanagedHeap
{
    public static class WindowsPrivateHeapIndirect
    {
        private static readonly IWindowsPrivateHeapNative Kernel32Lib;
        static WindowsPrivateHeapIndirect()
        {
            var kernel32Path = Path.Join(Environment.SystemDirectory, "kernel32.dll");
            var nativeLibBuilder = new NativeLibraryBuilder(ImplementationOptions.UseIndirectCalls|ImplementationOptions.EnableOptimizations);
            Kernel32Lib = nativeLibBuilder.ActivateInterface<IWindowsPrivateHeapNative>(kernel32Path);
            if (Kernel32Lib == null)
            {
                throw new UnmanagedHeapUnavailable("Failed to load kernel32.dll");
            }
        }
        
        public static IUnmanagedHeap Create()
        {
            return Create(4194304, 0);
        }

        public static IUnmanagedHeap Create(uint initialSize, uint maximumSize, uint options = (uint)WindowsHeapOptions.ZeroMemory, bool withCounter = false)
        {
            var handle = Kernel32Lib.HeapCreate(
                (uint) WindowsHeapOptions.ZeroMemory,
                (IntPtr) initialSize,
                (IntPtr) maximumSize);
            if (handle == IntPtr.Zero)
            {
                throw new UnmanagedHeapUnavailable("Failed to create heap");
            }

            return new WindowsPrivateHeap(handle, Kernel32Lib);
        }
    }
}