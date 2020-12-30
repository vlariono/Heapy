using System;
using System.Runtime.CompilerServices;
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
        private IntPtr _memory;
        private bool _disposed;

        public unsafe Unmanaged(IntPtr memory, int length, IUnmanagedHeap heap) : this()
        {
            if (heap == null)
            {
                throw new ArgumentNullException(nameof(heap));
            }

            if (memory == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            _memory = memory;
            _heap = heap;
            _span = new Span<TValue>((TValue*)memory, length);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _heap.Free(_memory);
                _memory = IntPtr.Zero;
                _disposed = true;
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
                return _memory;
            }
        }

        /// <summary>
        /// Returns heap where memory is allocated <see cref="IUnmanagedHeap"/>
        /// </summary>
        public IUnmanagedHeap Heap => _heap;

        /// <summary>
        /// Shortcut to first element in memory block
        /// </summary>
        public ref TValue First => ref this[0];

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
            return _span.IndexOfByValue(ref value);
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

        public void ThrowIfNotAvailable()
        {
            if (_disposed)
            {
                throw new UnmanagedObjectUnavailable("Object has been disposed or heap is unavailable");
            }

            _heap.ThrowIfNotAvailable();
        }

        #region Unsupported
        [Obsolete("The method is unsupported")]
        public override bool Equals(object? obj)
        {
            throw new NotSupportedException(nameof(Equals));
        }

        [Obsolete("The method is unsupported")]
        public override int GetHashCode()
        {
            throw new NotSupportedException(nameof(GetHashCode));
        }

        [Obsolete("The method is unsupported")]
        public override string? ToString()
        {
            throw new NotImplementedException(nameof(ToString));
        }
        #endregion

        public static implicit operator IntPtr(Unmanaged<TValue> unmanagedValue) => unmanagedValue.Ptr;
        public static unsafe implicit operator TValue*(Unmanaged<TValue> unmanagedValue) => (TValue*)unmanagedValue.Ptr;
        public static implicit operator Span<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();
        public static implicit operator ReadOnlySpan<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();
    }
}