using System;
using Heapy.Core.Exception;
using Heapy.Core.Extension;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    public unsafe ref struct Expandable<TValue> where TValue : unmanaged
    {
        private readonly IUnmanagedHeap _heap;

        private int _length;
        private int _count;
        private bool _isFixed;
        private TValue* _memory;
        private bool _disposed;

        public Expandable(int length, IUnmanagedHeap heap):this()
        {
            _length = length;
            _heap = heap;
            _memory = heap.Alloc<TValue>(length);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _heap.Free((IntPtr)_memory);
                _memory = (TValue*)IntPtr.Zero;
                _disposed = true;
            }
        }

        /// <summary>
        /// Returns length of memory block
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns current number of items in the memory block
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Returns heap where memory is allocated <see cref="IUnmanagedHeap"/>
        /// </summary>
        public IUnmanagedHeap Heap => _heap;

        /// <summary>
        /// Returns true is memory block has fixed size
        /// </summary>
        public bool IsFixed => _isFixed;

        /// <summary>
        /// Returns unmanaged pointer to unmanaged memory block
        /// Once pointer is returned memory block will have fixed size
        /// </summary>
        public IntPtr Ptr
        {
            get
            {
                _isFixed = true;
                return (IntPtr) _memory;
            }
        }

        /// <summary>
        /// Resize memory block
        /// </summary>
        /// <param name="length">Length of new memory block</param>
        public void Resize(int length)
        {
            ThrowIfNotAvailable();
            if (_isFixed)
            {
                throw new InvalidOperationException("Memory block is fixed and cannot be resized");
            }
            _memory = _heap.Realloc<TValue>((IntPtr)_memory, length);
            _length = length;
        }

        /// <summary>
        /// Gets the element at the specified zero-based index
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns><see cref="TValue"/></returns>
        public TValue this[int index]
        {
            get => GetByIndex(index);
        }

        /// <summary>
        /// Sets item at index to <see cref="value"/>
        /// </summary>
        /// <param name="index">Index of item</param>
        /// <param name="value">value to set</param>
        public void Update(int index, TValue value)
        {
            ThrowIfNotAvailable();
            ThrowIfOutOfRange(index);
            _memory[index] = value;
        }
        
        /// <summary>
        /// Adds an item to the end of the memory block
        /// </summary>
        /// <param name="item">The item to be added</param>
        public void Add(TValue item)
        {
            ThrowIfNotAvailable();
            if (_count >= _length)
            {
                Resize(Math.Max(1, _length * 2));
            }

            _memory[_count++] = item;
        }

        /// <summary>
        /// Adds the items of the span to the end of memory block
        /// </summary>
        /// <param name="values">Items to be added to memory block</param>
        public void AddRange(ReadOnlySpan<TValue> values)
        {
            ThrowIfNotAvailable();
            var requiredCount = _count + values.Length;
            if (requiredCount >= _length)
            {
                Resize(requiredCount);
            }

            var span = new Span<TValue>(_memory, _length).Slice(_count);
            values.CopyTo(span);
            _count = requiredCount;
        }

        /// <summary>
        /// Removes the element at the specified index
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="index">The zero-based index of the element to remove</param>
        public void RemoveAt(int index)
        {
            ThrowIfNotAvailable();
            ThrowIfOutOfRange(index);
            new Span<TValue>(_memory, _count).RemoveAt(index);
            _count--;
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence
        /// </summary>
        /// <param name="item">The value to search for</param>
        /// <returns>If succeeded - index of item, otherwise -1</returns>
        public int IndexOf(TValue item)
        {
            ThrowIfNotAvailable();
            var span = new Span<TValue>(_memory, _count);
            return span.IndexOfByValue(ref item);
        }

        /// <summary>
        /// Copies from <see cref="Unmanaged{TValue}"/> to <see cref="Span{T}"/>
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="span">Destination <see cref="Span{T}"/></param>
        public void CopyTo(Span<TValue> span)
        {
            ThrowIfNotAvailable();
            var sourceSpan = new Span<TValue>(_memory, _count);
            sourceSpan.CopyTo(span);
        }

        /// <summary>
        /// Returns a span over unmanaged memory
        /// Once span is returned memory block will have fixed size
        /// </summary>
        /// <returns><see cref="Span{T}"/></returns>
        public Span<TValue> AsSpan()
        {
            _isFixed = true;
            return new(_memory, _count);
        }

        private TValue GetByIndex(int index)
        {
            ThrowIfNotAvailable();
            ThrowIfOutOfRange(index);
            return _memory[index];
        }

        private void ThrowIfOutOfRange(int index)
        {
            if (0 > index || index >= _count)
            {
                throw new IndexOutOfRangeException(index.ToString());
            }
        }

        private void ThrowIfNotAvailable()
        {
            if (_disposed)
            {
                throw new UnmanagedObjectUnavailable();
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
    }
}