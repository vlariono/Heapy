using System;
using System.Runtime.InteropServices;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Heap;
using Heapy.Core.Interface;
using Heapy.WindowsHeap.Interface;

namespace Heapy.WindowsHeap.Heap
{
    public sealed class WindowsPrivateHeap:IUnmanagedHeap
    {
        private readonly WindowsHeapHandle _heapHandle;
        private readonly IWindowsPrivateHeapNative _kernel32Lib;
        private readonly bool _withCounter;
        private bool _disposed;
        public WindowsPrivateHeap(IntPtr handle, IWindowsPrivateHeapNative kernel32Lib)
        {
            _disposed = false;
            _withCounter = false;
            _heapHandle = new WindowsHeapHandle(handle,kernel32Lib);
            _kernel32Lib = kernel32Lib;
        }

        public WindowsPrivateHeap(IntPtr handle, IWindowsPrivateHeapNative kernel32Lib, bool withCounter):this(handle,kernel32Lib)
        {
            _withCounter = withCounter;
        }

        private void ThrowIfUnavailable()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WindowsPrivateHeap));
            }
            
            if (State != UnmanagedState.Available)
            {
                throw new UnmanagedHeapUnavailable();
            }
        }

        public bool IsReadonly => _disposed && _heapHandle.State == UnmanagedState.Available;
        public UnmanagedState State => _heapHandle.State;

        public void Dispose()
        {
            if(_heapHandle.State == UnmanagedState.Available)
            {
                _heapHandle.Dispose();
            }

            _disposed = true;
        }

        public unsafe Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>((uint)sizeof(TValue),(uint) WindowsHeapOptions.None);
        }

        public unsafe Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var unmanagedObject = Alloc<TValue>((uint)sizeof(TValue),(uint) WindowsHeapOptions.None);
            Marshal.StructureToPtr(value, unmanagedObject, true);
            return unmanagedObject;
        }

        public Unmanaged<TValue> Alloc<TValue>(uint bytes) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Alloc<TValue>(bytes, (uint) WindowsHeapOptions.None);
        }

        public Unmanaged<TValue> Alloc<TValue>(uint bytes, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = IntPtr.Zero;
            var setCounter =false;
            try
            {
                allocHandle = _kernel32Lib.HeapAlloc(_heapHandle.DangerousGetHandle(), options, bytes);
                if (allocHandle == IntPtr.Zero)
                {
                    throw new UnmanagedObjectUnavailable("Failed to allocate memory");
                }

                if (_withCounter)
                {
                    _heapHandle.DangerousAddRef(ref setCounter);
                    if (!setCounter)
                    {
                        throw new UnmanagedObjectUnavailable("Failed to increase object counter");
                    }
                }

                return new Unmanaged<TValue>
                {
                    Ptr = allocHandle,
                    Heap = this
                };
            }
            catch
            {
                if (allocHandle != IntPtr.Zero)
                {
                    _kernel32Lib.HeapFree(_heapHandle.DangerousGetHandle(), 0, allocHandle);
                }

                if (setCounter)
                {
                    _heapHandle.DangerousRelease();
                }

                throw;
            }
        }

        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            return Realloc<TValue>(memory, bytes, (uint) WindowsHeapOptions.None);
        }

        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes, uint options) where TValue : unmanaged
        {
            ThrowIfUnavailable();
            var allocHandle = _kernel32Lib.HeapReAlloc(_heapHandle.DangerousGetHandle(), options, memory, bytes);
            if (allocHandle == IntPtr.Zero)
            {
                throw new UnmanagedObjectUnavailable("Failed to reallocate memory");
            }
            
            return new Unmanaged<TValue>
            {
                Ptr = allocHandle,
                Heap = this
            };
        }

        public bool Free(IntPtr memory)
        {
            if (_heapHandle.State != UnmanagedState.Available)
            {
                return false;
            }
            
            bool result;
            try
            {
                result = _kernel32Lib.HeapFree(_heapHandle.DangerousGetHandle(), 0, memory);
            }
            finally
            {
                if (_withCounter)
                {
                    _heapHandle.DangerousRelease();
                }
            }

            return result;
        }
    }
}