using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;
using Heapy.WindowsHeap.Interface;

namespace Heapy.WindowsHeap.UnmanagedHeap
{
    public sealed class WindowsPrivateHeap : IUnmanagedHeap
    {
        private readonly IWindowsPrivateHeapNative _kernel32Lib;
        private IntPtr _handle;
        private UnmanagedState _state;

        public WindowsPrivateHeap(IntPtr handle, IWindowsPrivateHeapNative kernel32Lib)
        {
            _handle = handle;
            _kernel32Lib = kernel32Lib;
            _state = handle == IntPtr.Zero? UnmanagedState.Unavailable: UnmanagedState.Available;
        }

        private void ThrowIfUnavailable()
        {
            if (_handle == IntPtr.Zero)
            {
                throw new UnmanagedHeapUnavailable("Heap is readonly or disposed");
            }
        }

        public UnmanagedState State => _state;

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                _kernel32Lib.HeapDestroy(_handle);
                _handle = IntPtr.Zero;
                _state = UnmanagedState.Unavailable;
            }
            GC.SuppressFinalize(this);
        }

        ~WindowsPrivateHeap()
        {
            Dispose();
        }

        public unsafe Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>((uint)sizeof(TValue), (uint)WindowsHeapOptions.None);
        }

        public unsafe Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var unmanagedObject = Alloc<TValue>((uint)sizeof(TValue), (uint)WindowsHeapOptions.None);
            unmanagedObject.Value = value;
            return unmanagedObject;
        }

        public Unmanaged<TValue> Alloc<TValue>(uint bytes) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>(bytes, (uint)WindowsHeapOptions.None);
        }

        public Unmanaged<TValue> Alloc<TValue>(uint bytes, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = _kernel32Lib.HeapAlloc(_handle, options, bytes);
            return allocHandle == IntPtr.Zero
                ? throw new OutOfMemoryException("Failed to allocate memory")
                : new Unmanaged<TValue>(allocHandle, this);
        }

        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Realloc<TValue>(memory, bytes, (uint)WindowsHeapOptions.None);
        }

        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = _kernel32Lib.HeapReAlloc(_handle, options, memory, bytes);
            return allocHandle == IntPtr.Zero
                ? throw new OutOfMemoryException("Failed to allocate memory")
                : new Unmanaged<TValue>(allocHandle, this);
        }

        public bool Free(IntPtr memory)
        {
            return _handle != IntPtr.Zero && _kernel32Lib.HeapFree(_handle, 0, memory);
        }
    }
}