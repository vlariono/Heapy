using System;
using System.Runtime.InteropServices;
using Heapy.Core.Enum;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Represents instance of default process heap. This heap is not private and do not support real disposal
    /// </summary>
    public sealed class ProcessHeap : IUnmanagedHeap
    {
        private bool _disposed;
        public ProcessHeap()
        {
            _disposed = false;
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private void ThrowIfUnavailable()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ProcessHeap));
            }
        }

        /// <summary>
        /// Creates new instance of PrivateHeap
        /// </summary>
        /// <returns><see cref="IUnmanagedHeap"/></returns>
        public static IUnmanagedHeap Create()
        {
            return new ProcessHeap();
        }

        /// <inheritdoc />
        public UnmanagedState State => UnmanagedState.Available;
        
        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>(1);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var unmanagedValue = Alloc<TValue>(1);
            unmanagedValue[0] = value;
            return unmanagedValue;
        }

        /// <inheritdoc />
        public unsafe Unmanaged<TValue> Alloc<TValue>(int length) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var cb = new IntPtr(sizeof(TValue) * length);
            var allocHandle = Marshal.AllocHGlobal(cb);
            return new Unmanaged<TValue>(allocHandle, length, this);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>(int length, uint options) where TValue : unmanaged
        {
            return Alloc<TValue>(length);
        }

        /// <inheritdoc />
        public unsafe Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var cb = new IntPtr(sizeof(TValue) * length);
            var allocHandle = Marshal.ReAllocHGlobal(memory, cb);
            return new Unmanaged<TValue>(allocHandle,length, this);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length, uint options) where TValue : unmanaged
        {
            return Realloc<TValue>(memory, length);
        }

        /// <inheritdoc />
        public bool Free(IntPtr memory)
        {
            Marshal.FreeHGlobal(memory);
            return true;
        }
    }
}