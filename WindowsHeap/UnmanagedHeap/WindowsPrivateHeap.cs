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
        private readonly WindowsHeapHandle _heapHandle;
        private readonly IWindowsPrivateHeapNative _kernel32Lib;
        private bool _disposed;
        public WindowsPrivateHeap(IntPtr handle, IWindowsPrivateHeapNative kernel32Lib)
        {
            _disposed = false;
            _heapHandle = new WindowsHeapHandle(handle, kernel32Lib);
            _kernel32Lib = kernel32Lib;
        }

        private void ThrowIfUnavailable()
        {
            if (_disposed || State != UnmanagedState.Available)
            {
                throw new UnmanagedHeapUnavailable("Heap is readonly or disposed");
            }
        }

        public UnmanagedState State => _heapHandle.State;

        public void Dispose()
        {
            if (_heapHandle.State == UnmanagedState.Available)
            {
                _heapHandle.Dispose();
            }

            _disposed = true;
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
            var allocHandle = _kernel32Lib.HeapAlloc(_heapHandle.DangerousGetHandle(), options, bytes);
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
            var allocHandle = _kernel32Lib.HeapReAlloc(_heapHandle.DangerousGetHandle(), options, memory, bytes);
            return allocHandle == IntPtr.Zero
                ? throw new OutOfMemoryException("Failed to allocate memory")
                : new Unmanaged<TValue>(allocHandle, this);
        }

        public bool Free(IntPtr memory)
        {
            return _heapHandle.State == UnmanagedState.Available && _kernel32Lib.HeapFree(_heapHandle.DangerousGetHandle(), 0, memory);
        }
    }
}