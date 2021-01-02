using System;
using System.IO;
using AdvancedDLSupport;
using Heapy.Core;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.WindowsHeapIndirect.UnmanagedHeap
{
    public static class WindowsPrivateHeapIndirect
    {
        private static readonly IPrivateHeapNative Kernel32Lib;
        static WindowsPrivateHeapIndirect()
        {
            var kernel32Path = Path.Join(Environment.SystemDirectory, "kernel32.dll");
            var nativeLibBuilder = new NativeLibraryBuilder(ImplementationOptions.UseIndirectCalls|ImplementationOptions.EnableOptimizations);
            Kernel32Lib = nativeLibBuilder.ActivateInterface<IPrivateHeapNative>(kernel32Path);
            if (Kernel32Lib == null)
            {
                throw new UnmanagedHeapUnavailable("Failed to load kernel32.dll");
            }
        }

        public static IUnmanagedHeap Create()
        {
            return Create(4194304, 0);
        }

        public static IUnmanagedHeap Create(uint initialSize, uint maximumSize, uint options = (uint)WindowsHeapOptions.Default)
        {
            var handle = Kernel32Lib.HeapCreate(
                options,
                (IntPtr) initialSize,
                (IntPtr) maximumSize);
            if (handle == IntPtr.Zero)
            {
                throw new UnmanagedHeapUnavailable("Failed to create heap");
            }

            return new PrivateHeap(handle, Kernel32Lib);
        }
    }
}