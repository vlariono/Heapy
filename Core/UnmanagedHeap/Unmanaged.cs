using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Represents contiguous region of allocated unmanaged memory
    /// </summary>
    /// <typeparam name="TValue">The type of items in the unmanaged memory/></typeparam>
    public unsafe ref struct Unmanaged<TValue> where TValue : unmanaged
    {
        private readonly IUnmanagedHeap _heap;
        private readonly int _length;
        private TValue* _value;

        public Unmanaged(IntPtr ptr, int length,IUnmanagedHeap heap) : this()
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
            _length = length;
            _value = (TValue*)ptr;
        }

        /// <summary>
        /// Returns length of the allocated memory
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns heap where memory is allocated <see cref="IUnmanagedHeap"/>
        /// </summary>
        public IUnmanagedHeap Heap => _heap;

        /// <summary>
        /// Returns state of allocated memory <see cref="UnmanagedState"/>
        /// </summary>
        public UnmanagedState State => (IntPtr)_value == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        /// <summary>
        /// Returns reference to first element
        /// </summary>
        public ref TValue Value => ref this[0];

        /// <summary>
        /// Gets the element at the specified zero-based index
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns><see cref="TValue"/></returns>
        public ref TValue this[int index]
        {
            get
            {
                if (State != UnmanagedState.Available)
                {
                    throw new UnmanagedObjectUnavailable();
                }

                if (0 > index || index>= _length)
                {
                    throw new IndexOutOfRangeException(index.ToString());
                }

                return ref _value[index];
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

        /// <summary>
        /// Creates a new span over unmanaged memory
        /// </summary>
        /// <returns><see cref="Span{T}"/></returns>
        public Span<TValue> AsSpan() => new(_value, _length);

        public static implicit operator IntPtr(Unmanaged<TValue> unmanagedValue) => (IntPtr)unmanagedValue._value;
        public static implicit operator TValue*(Unmanaged<TValue> unmanagedValue) => unmanagedValue._value;
        public static implicit operator TValue(Unmanaged<TValue> unmanagedValue) => unmanagedValue.Value;
        public static implicit operator Span<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();
    }
}