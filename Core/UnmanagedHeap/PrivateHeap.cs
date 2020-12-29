using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Represent instance of platform specific private heap. Private heap supports disposal
    /// If the heap is disposed all previously allocated memory blocks in this heap are gone forever
    /// </summary>
    public sealed class PrivateHeap : IUnmanagedHeap
    {
        private readonly IPrivateHeapNative _privateHeapNative;
        private IntPtr _handle;
        private UnmanagedState _state;

        /// <summary>
        /// Initializes new instance of private heap
        /// </summary>
        /// <param name="handle">Handle of private heap</param>
        /// <param name="kernel32Lib">Heap API implementation</param>
        public PrivateHeap(IntPtr handle, IPrivateHeapNative kernel32Lib)
        {
            _handle = handle;
            _privateHeapNative = kernel32Lib;
            _state = handle == IntPtr.Zero ? UnmanagedState.Unavailable : UnmanagedState.Available;
        }

        /// <inheritdoc />
        public UnmanagedState State => _state;

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                _privateHeapNative.HeapDestroy(_handle);
                _handle = IntPtr.Zero;
                _state = UnmanagedState.Unavailable;
            }
            GC.SuppressFinalize(this);
        }

        ~PrivateHeap()
        {
            Dispose();
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            return Alloc<TValue>(1,(uint) WindowsHeapOptions.Default);
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
            var allocHandle = _privateHeapNative.HeapAlloc(_handle, options, (IntPtr)(sizeof(TValue) * length));
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
            var allocHandle = _privateHeapNative.HeapReAlloc(_handle, options, memory, (IntPtr)(sizeof(TValue) * length));
            ThrowIfOutOfMemory(allocHandle);
            return new Unmanaged<TValue>(allocHandle, length, this);
        }

        /// <inheritdoc />
        public bool Free(IntPtr memory)
        {
            return _handle != IntPtr.Zero && _privateHeapNative.HeapFree(_handle, 0, memory);
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