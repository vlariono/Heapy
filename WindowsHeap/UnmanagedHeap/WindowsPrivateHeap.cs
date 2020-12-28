using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;
using Heapy.WindowsHeap.Interface;

namespace Heapy.WindowsHeap.UnmanagedHeap
{
    /// <summary>
    /// Represent instance of windows specific private heap. Private heap supports disposal
    /// If the heap is disposed all previously allocated memory blocks in this heap are gone forever
    /// </summary>
    public sealed class WindowsPrivateHeap : IUnmanagedHeap
    {
        private readonly IWindowsPrivateHeapNative _kernel32Lib;
        private IntPtr _handle;
        private UnmanagedState _state;

        /// <summary>
        /// Initializes new instance of private heap
        /// </summary>
        /// <param name="handle">Handle of private heap</param>
        /// <param name="kernel32Lib">Heap API implementation</param>
        public WindowsPrivateHeap(IntPtr handle, IWindowsPrivateHeapNative kernel32Lib)
        {
            _handle = handle;
            _kernel32Lib = kernel32Lib;
            _state = handle == IntPtr.Zero ? UnmanagedState.Unavailable : UnmanagedState.Available;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            return Alloc<TValue>(1,(uint) WindowsHeapOptions.Default);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged
        {
            var unmanagedValue = Alloc<TValue>(1,(uint) WindowsHeapOptions.Default);
            unmanagedValue[0] = value;
            return unmanagedValue;
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>(int length) where TValue : unmanaged
        {
            return Alloc<TValue>(length,(uint) WindowsHeapOptions.Default);
        }

        /// <inheritdoc />
        public unsafe Unmanaged<TValue> Alloc<TValue>(int length, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = _kernel32Lib.HeapAlloc(_handle, options, (IntPtr)(sizeof(TValue) * length));
            ThrowIfOutOfMemory(allocHandle);
            return new Unmanaged<TValue>(allocHandle, length, this);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length) where TValue : unmanaged
        {
            return Realloc<TValue>(memory, length, (uint) WindowsHeapOptions.Default);
        }

        /// <inheritdoc />
        public unsafe Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = _kernel32Lib.HeapReAlloc(_handle, options, memory, (IntPtr)(sizeof(TValue) * length));
            ThrowIfOutOfMemory(allocHandle);
            return new Unmanaged<TValue>(allocHandle, length, this);
        }

        /// <inheritdoc />
        public bool Free(IntPtr memory)
        {
            return _handle != IntPtr.Zero && _kernel32Lib.HeapFree(_handle, 0, memory);
        }

        private void ThrowIfUnavailable()
        {
            if (_handle == IntPtr.Zero)
            {
                throw new UnmanagedHeapUnavailable("Heap is readonly or disposed");
            }
        }

        private static void ThrowIfOutOfMemory(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new OutOfMemoryException("Failed to allocate memory");
            }
        }
    }
}