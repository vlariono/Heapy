using System;
using System.Runtime.CompilerServices;
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
        private bool _disposed;

        /// <summary>
        /// Initializes new instance of private heap
        /// </summary>
        /// <param name="handle">Handle of private heap</param>
        /// <param name="privateHeapNative">Heap API implementation</param>
        public PrivateHeap(IntPtr handle, IPrivateHeapNative privateHeapNative)
        {
            _handle = handle;
            _privateHeapNative = privateHeapNative;
        }

        /// <inheritdoc />
        //public UnmanagedState State => _state;

        public void Dispose()
        {
            if (!_disposed)
            {
                _privateHeapNative.HeapDestroy(_handle);
                _handle = IntPtr.Zero;
                _disposed = true;
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
            ThrowIfNotAvailable();
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
            ThrowIfNotAvailable();
            var allocHandle = _privateHeapNative.HeapReAlloc(_handle, options, memory, (IntPtr)(sizeof(TValue) * length));
            ThrowIfOutOfMemory(allocHandle);
            return new Unmanaged<TValue>(allocHandle, length, this);
        }

        /// <inheritdoc />
        public bool Free(IntPtr memory)
        {
            return _handle != IntPtr.Zero && _privateHeapNative.HeapFree(_handle, 0, memory);
        }

        public void ThrowIfNotAvailable()
        {
            if (_disposed)
            {
                throw new UnmanagedHeapUnavailable("Private heap is unavailable");
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