using System;
using System.Collections.Generic;
using Heapy.Core.Enum;
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

        public Expandable(int length, IUnmanagedHeap heap)
        {
            _count = 0;
            _isFixed = false;
            _length = length;
            _heap = heap;
            _memory = heap.Alloc<TValue>(length);
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                _heap.Free((IntPtr)_memory);
                _memory = (TValue*)IntPtr.Zero;
            }
        }

        public UnmanagedState State => (IntPtr)_memory == IntPtr.Zero || _heap.State != UnmanagedState.Available
            ? UnmanagedState.Unavailable
            : UnmanagedState.Available;

        /// <summary>
        /// Returns length of memory block
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns current number of items in the memory block
        /// </summary>
        public int Count => _count;
        public IUnmanagedHeap Heap => _heap;
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

        public void Resize(int length)
        {
            ThrowIfUnavailable();
            if (_isFixed)
            {
                throw new InvalidOperationException("Memory block is fixed and cannot be resized");
            }
            _memory = _heap.Realloc<TValue>((IntPtr)_memory, length);
            _length = length;
        }

        public TValue this[int index]
        {
            get => GetByIndex(index);
            set => SetByIndex(index, value);
        }

        public void Add(TValue item)
        {
            ThrowIfUnavailable();
            if (_count >= _length)
            {
                Resize(Math.Max(1, _length * 2));
            }

            _memory[_count++] = item;
        }

        public void AddRange(ReadOnlySpan<TValue> values)
        {
            ThrowIfUnavailable();
            var requiredCount = _count + values.Length;
            if (requiredCount >= _length)
            {
                Resize(requiredCount);
            }

            var span = new Span<TValue>(_memory, _length);
            values.CopyTo(span);
            _count = requiredCount;
        }

        public void RemoveAt(int index)
        {
            ThrowIfUnavailable();
            ThrowIfOutOfRange(index);
            new Span<TValue>(_memory, _count).RemoveAt(index);
            _count--;
        }

        public int IndexOf(TValue item)
        {
            ThrowIfUnavailable();
            var span = new Span<TValue>(_memory, _count);
            return span.IndexOfByValue(ref item);
        }

        /// <summary>
        /// Returns a span over unmanaged memory
        /// Once span is returned memory block will have fixed size
        /// </summary>
        /// <returns><see cref="Span{T}"/></returns>
        internal Span<TValue> AsSpan()
        {
            _isFixed = true;
            return new(_memory, _count);
        }

        private TValue GetByIndex(int index)
        {
            ThrowIfUnavailable();
            ThrowIfOutOfRange(index);
            return _memory[index];
        }

        private void SetByIndex(int index, TValue value)
        {
            ThrowIfUnavailable();
            ThrowIfOutOfRange(index);
            _memory[index] = value;
        }

        private void ThrowIfOutOfRange(int index)
        {
            if (0 > index || index >= _count)
            {
                throw new IndexOutOfRangeException(index.ToString());
            }
        }

        private void ThrowIfUnavailable()
        {
            if (State != UnmanagedState.Available)
            {
                throw new UnmanagedObjectUnavailable();
            }
        }
    }
}