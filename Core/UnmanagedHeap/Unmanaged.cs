using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Extension;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Wrapper around contiguous region of allocated unmanaged memory
    /// </summary>
    /// <typeparam name="TValue">The type of items in the unmanaged memory/></typeparam>
    public ref struct Unmanaged<TValue> where TValue : unmanaged
    {
        private readonly IUnmanagedHeap _heap;
        private readonly Span<TValue> _span;
        private IntPtr _ptr;

        public unsafe Unmanaged(IntPtr ptr, int length, IUnmanagedHeap heap) : this()
        {
            if (heap == null)
            {
                throw new ArgumentNullException(nameof(heap));
            }

            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            _ptr = ptr;
            _heap = heap;
            _span = new Span<TValue>((TValue*)ptr, length);
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                _heap.Free(_ptr);
                _ptr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns length of the allocated memory
        /// </summary>
        public int Length
        {
            get
            {
                ThrowIfNotAvailable();
                return _span.Length;
            }
        }

        /// <summary>
        /// Returns unmanaged pointer to unmanaged memory block
        /// </summary>
        public IntPtr Ptr
        {
            get
            {
                ThrowIfNotAvailable();
                return _ptr;
            }
        }

        /// <summary>
        /// Returns heap where memory is allocated <see cref="IUnmanagedHeap"/>
        /// </summary>
        public IUnmanagedHeap Heap => _heap;

        /// <summary>
        /// Returns state of allocated memory <see cref="UnmanagedState"/>
        /// </summary>
        public UnmanagedState State => _ptr == IntPtr.Zero || _heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        /// <summary>
        /// Gets the element at the specified zero-based index
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns><see cref="TValue"/></returns>
        public ref TValue this[int index]
        {
            get
            {
                ThrowIfNotAvailable();
                return ref _span[index];
            }
        }

        /// <summary>
        /// Returns a span over unmanaged memory
        /// </summary>
        /// <returns><see cref="Span{T}"/></returns>
        public Span<TValue> AsSpan()
        {
            ThrowIfNotAvailable();
            return _span;
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>If succeeded - index of item, otherwise -1</returns>
        public int IndexOf(TValue value)
        {
            ThrowIfNotAvailable();
            return ((ReadOnlySpan<TValue>) _span).IndexOfByValue(value);
        }

        /// <summary>
        /// Removes the element at the specified index
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="index">The zero-based index of the element to remove</param>
        public void RemoveAt(int index)
        {
            ThrowIfNotAvailable();
            _span.RemoveAt(index);
        }

        /// <summary>
        /// Copies from <see cref="Unmanaged{TValue}"/> to <see cref="Span{T}"/>
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="span">Destination <see cref="Span{T}"/></param>
        public void CopyTo(Span<TValue> span)
        {
            ThrowIfNotAvailable();
            _span.CopyTo(span);
        }

        private void ThrowIfNotAvailable()
        {
            if (State == UnmanagedState.Available)
            {
                return;
            }
            throw new UnmanagedObjectUnavailable("Object has been disposed or heap is unavailable");
        }

        public override bool Equals(object? obj)
        {
            throw new NotSupportedException(nameof(Equals));
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException(nameof(GetHashCode));
        }

        public override string? ToString()
        {
            throw new NotImplementedException(nameof(ToString));
        }

        public static implicit operator IntPtr(Unmanaged<TValue> unmanagedValue) => unmanagedValue.Ptr;
        public static unsafe implicit operator TValue*(Unmanaged<TValue> unmanagedValue) => (TValue*)unmanagedValue.Ptr;
        public static implicit operator Span<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();
        public static implicit operator ReadOnlySpan<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();
    }
}