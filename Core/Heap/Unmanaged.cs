using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.Heap
{
    public unsafe ref struct Unmanaged<TValue> where TValue : unmanaged
    {
        private readonly TValue* _value;
        private Managed<TValue>? _managed;
        private bool _disposed;

        public Unmanaged(TValue value) : this()
        {
            Heap = HeapManager.Current;
            Ptr = Heap.Alloc(value);
        }

        public Unmanaged(uint bytes) : this()
        {
            Heap = HeapManager.Current;
            Ptr = Heap.Alloc<TValue>(bytes);
        }

        public IntPtr Ptr
        {
            private get => (IntPtr)_value;
            init => _value = (TValue*)value;
        }

        public IUnmanagedHeap Heap { get; init; }

        public UnmanagedState State => _disposed || Ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        public ref TValue Value
        {
            get
            {
                if (_managed != null)
                {
                    throw new UnmanagedObjectUnavailable("Managed object has been created");
                }

                if (State != UnmanagedState.Available)
                {
                    throw new UnmanagedObjectUnavailable();
                }

                return ref *_value;
            }
        }

        public Managed<TValue> Managed
        {
            get
            {
                if (State != UnmanagedState.Available)
                {
                    throw new UnmanagedObjectUnavailable();
                }

                return _managed ??= new Managed<TValue>(this);
            }
        }

        public void Dispose()
        {
            if (_managed == null && State == UnmanagedState.Available)
            {
                Heap.Free(Ptr);
            }

            _disposed = true;
        }

        public static implicit operator IntPtr(Unmanaged<TValue> unmanagedValue) => unmanagedValue.Ptr;
    }
}