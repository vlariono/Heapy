using System;
using System.Runtime.InteropServices;
using Heapy.Core.Exceptions;
namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Represents contiguous region of allocated unmanaged memory
    /// </summary>
    /// <typeparam name="TValue">The type of items in the unmanaged memory/></typeparam>
    public ref struct Unmanaged<TValue> where TValue : unmanaged
    {
        private readonly Span<TValue> _span;
        private readonly IntPtr _memory;
        private bool _disposed;

        private unsafe Unmanaged(int length) : this()
        {
            _memory = Marshal.AllocHGlobal(sizeof(TValue) * length);
            _span = new Span<TValue>((TValue*)_memory, length);
        }

        /// <summary>
        /// Allocates a block of memory from unmanaged heap
        /// </summary>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> Alloc() => new(1);

        /// <summary>
        /// Allocates a block of memory from unmanaged heap
        /// </summary>
        /// <param name="length">Number of items in the memory block</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> Alloc(int length) => new(length);

        public void Dispose()
        {
            if (!_disposed)
            {
                Marshal.FreeHGlobal(_memory);
                _disposed = true;
            }
        }

        /// <summary>
        /// Returns an enumerator
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
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

        private void ThrowIfNotAvailable()
        {
            if (_memory == default)
            {
                throw new UnmanagedObjectUnavailable("Unmanaged object is unavailable");
            }

            if (_disposed)
            {
                throw new ObjectDisposedException("Object has been disposed");
            }
        }

        public ref struct Enumerator
        {
            private readonly Unmanaged<TValue> _memory;
            private int _index;
            public Enumerator(Unmanaged<TValue> memory)
            {
                _memory = memory;
                _index = -1;
            }
            public ref TValue Current => ref _memory[_index];

            public bool MoveNext()
            {
                return ++_index < _memory.Length;
            }
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
            throw new NotSupportedException(nameof(ToString));
        }
        #endregion

        public static implicit operator IntPtr(Unmanaged<TValue> unmanagedValue) => unmanagedValue.Ptr;
        public static implicit operator Span<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();
        public static implicit operator ReadOnlySpan<TValue>(Unmanaged<TValue> unmanagedValue) => unmanagedValue.AsSpan();

        /// <summary>
        /// Compares objects by reference
        /// </summary>
        /// <param name="first">First object</param>
        /// <param name="second">Second object</param>
        /// <returns>Returns true if two objects refer to the same memory block</returns>
        public static bool operator ==(Unmanaged<TValue> first, Unmanaged<TValue> second) => first._memory == second._memory;

        /// <summary>
        /// Compares objects by reference
        /// </summary>
        /// <param name="first">First object</param>
        /// <param name="second">Second object</param>
        /// <returns>Returns true if two objects do not refer to the same memory block</returns>
        public static bool operator !=(Unmanaged<TValue> first, Unmanaged<TValue> second) => first._memory != second._memory;
    }
}