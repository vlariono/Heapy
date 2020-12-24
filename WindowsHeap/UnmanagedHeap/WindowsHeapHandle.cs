using System;
using System.Runtime.InteropServices;
using Heapy.Core.Enum;
using Heapy.WindowsHeap.Interface;

namespace Heapy.WindowsHeap.UnmanagedHeap
{
    internal sealed class WindowsHeapHandle : SafeHandle
    {
        private readonly IWindowsPrivateHeapNative _windowsPrivateHeapNative;

        public WindowsHeapHandle(IntPtr handle, IWindowsPrivateHeapNative windowsPrivateHeapNative) : base(IntPtr.Zero, true)
        {
            _windowsPrivateHeapNative = windowsPrivateHeapNative;
            SetHandle(handle);
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        public UnmanagedState State => IsClosed || IsInvalid ? UnmanagedState.Unavailable : UnmanagedState.Available;

        protected override bool ReleaseHandle()
        {
            return _windowsPrivateHeapNative.HeapDestroy(handle);
        }
    }
}