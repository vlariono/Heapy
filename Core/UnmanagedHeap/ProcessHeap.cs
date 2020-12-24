using System;
using System.Runtime.InteropServices;
using Heapy.Core.Enum;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Default process heap. Does not support disposal
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

        public static IUnmanagedHeap Create()
        {
            return new ProcessHeap();
        }

        public UnmanagedState State => UnmanagedState.Available;
        
        public unsafe Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>((uint)sizeof(TValue));
        }

        public unsafe Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var unmanagedObject = Alloc<TValue>((uint)sizeof(TValue));
            unmanagedObject.Value = value;
            return unmanagedObject;
        }

        public Unmanaged<TValue> Alloc<TValue>(uint bytes) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>(bytes, 0);
        }

        public Unmanaged<TValue> Alloc<TValue>(uint bytes, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = Marshal.AllocHGlobal((int)bytes);
            return new Unmanaged<TValue>(allocHandle, this);
        }

        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Realloc<TValue>(memory, bytes, 0);
        }

        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var cb = new IntPtr(bytes);
            var allocHandle = Marshal.ReAllocHGlobal(memory, cb);
            return new Unmanaged<TValue>(allocHandle, this);
        }

        public bool Free(IntPtr memory)
        {
            Marshal.FreeHGlobal(memory);
            return true;
        }
    }
}