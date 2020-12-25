using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Wrapper around unmanaged memory pointer. Use 'using' to prevent memory leak
    /// <example>using var mem = new Unmanaged(value);</example>
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public unsafe ref struct Unmanaged<TValue> where TValue : unmanaged
    {
        private readonly IUnmanagedHeap _heap;
        private TValue* _value;

        public Unmanaged(IntPtr ptr, IUnmanagedHeap heap) : this()
        {
            if (heap == null)
            {
                throw new ArgumentNullException(nameof(heap));
            }

            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }
            
            _heap = heap;
            _value = (TValue*)ptr;
        }

        public Unmanaged(TValue value) : this()
        {
            _heap = UnmanagedHeap.Heap.Current;
            _value = Heap.Alloc(value);
        }

        public IUnmanagedHeap Heap => _heap;

        public UnmanagedState State => (IntPtr)_value == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        public ref TValue Value
        {
            get
            {
                if (State != UnmanagedState.Available)
                {
                    throw new UnmanagedObjectUnavailable();
                }

                return ref *_value;
            }
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                Heap.Free(this);
                _value = (TValue*) IntPtr.Zero;
            }
        }

        public static implicit operator IntPtr(Unmanaged<TValue> unmanagedValue) => (IntPtr)unmanagedValue._value;
        public static implicit operator TValue*(Unmanaged<TValue> unmanagedValue) => unmanagedValue._value;
        public static implicit operator TValue(Unmanaged<TValue> unmanagedValue) => unmanagedValue.Value;
    }
}