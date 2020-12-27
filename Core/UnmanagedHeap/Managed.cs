using System;
using System.Collections;
using System.Collections.Generic;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Extension;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Wrapper around contiguous region of allocated unmanaged memory.
    /// Memory block dynamically grows by adding elements to it.
    /// This wrapper makes sense for relatively large objects.
    /// Otherwise it's cheaper to copy memory block to managed memory.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the unmanaged memory/></typeparam>
    public sealed unsafe class Managed<TValue> : ICollection<TValue>, IDisposable where TValue : unmanaged
    {
        private readonly IUnmanagedHeap _heap;

        private int _length;
        private TValue* _ptr;
        private int _count;
        private bool _isFixedSize;
        private uint _version;

        public Managed(Unmanaged<TValue> unmanagedValue)
        {
            _version = 0;
            _isFixedSize = false;
            _ptr = (TValue*)unmanagedValue.Ptr;
            _heap = unmanagedValue.Heap;
            _count = _length = unmanagedValue.Length;
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                _heap.Free((IntPtr) _ptr);
                _ptr = (TValue*)IntPtr.Zero;
            }
        }

        ~Managed()
        {
            Dispose();
        }

        /// <inheritdoc />
        public int Count => _count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the element at the specified zero-based index
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns><see cref="TValue"/></returns>
        public TValue this[int index]
        {
            get
            {
                ThrowIfNotAvailable();
                ThrowIfOutOfRange(index);
                return _ptr[index];
            }
            set
            {
                ThrowIfNotAvailable();
                ThrowIfOutOfRange(index);
                _ptr[index] = value;
            }
        }

        /// <summary>
        /// Returns state of allocated memory <see cref="UnmanagedState"/>
        /// </summary>
        public UnmanagedState State => (IntPtr)_ptr == IntPtr.Zero || _heap.State != UnmanagedState.Available
            ? UnmanagedState.Unavailable
            : UnmanagedState.Available;

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Add(TValue item)
        {
            ThrowIfNotAvailable();
            EnsureSize();
            _version++;
            _ptr[_count++] = item;
        }

        /// <inheritdoc />
        public void Clear()
        {
            ThrowIfNotAvailable();
            _version++;
            new Span<TValue>(_ptr, _length).Clear();
        }

        /// <inheritdoc />
        public bool Contains(TValue item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            ThrowIfNotAvailable();
            var sourceSpan = new Span<TValue>(_ptr, _count);
            var destinationSpan = new Span<TValue>(array).Slice(arrayIndex);
            sourceSpan.CopyTo(destinationSpan);
        }

        /// <inheritdoc />
        public bool Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the element by reference at the specified zero-based index
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns>ref <see cref="TValue"/></returns>
        public ref TValue GetRef(int index)
        {
            ThrowIfNotAvailable();
            ThrowIfNotFixed();
            ThrowIfOutOfRange(index);
            return ref _ptr[index];
        }

        /// <summary>
        /// Gets unmanaged pointer to unmanaged memory block
        /// </summary>
        /// <returns><see cref="TValue"/></returns>
        public TValue* GetPointer()
        {
            ThrowIfNotAvailable();
            ThrowIfNotFixed();
            return _ptr;
        }

        /// <summary>
        /// Gets <see cref="IntPtr"/> which contains address of unmanaged memory block
        /// </summary>
        /// <returns><see cref="TValue"/></returns>
        public IntPtr GetPtr()
        {
            ThrowIfNotAvailable();
            ThrowIfNotFixed();
            return (IntPtr) _ptr;
        }

        /// <summary>
        /// Creates a new span over unmanaged memory
        /// </summary>
        /// <returns><see cref="Span{T}"/></returns>
        public Span<TValue> AsSpan()
        {
            ThrowIfNotAvailable();
            ThrowIfNotFixed();
            return new Span<TValue>(_ptr, _count);
        }

        /// <summary>
        /// Removes the element at the specified index
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="index">The zero-based index of the element to remove</param>
        public void RemoveAt(int index)
        {
            ThrowIfNotAvailable();
            _version++;
            new Span<TValue>(_ptr,_count).RemoteAt(index);
        }

        /// <summary>
        /// Locks length of unmanaged memory block
        /// </summary>
        public void SetMemoryFixed()
        {
            _isFixedSize = true;
        }

        public void Resize(int length)
        {
            if (_isFixedSize)
            {
                throw new InvalidOperationException("Memory block is fixed size");
            }

            _version++;
            _ptr = _heap.Realloc<TValue>((IntPtr)_ptr, length);
            _length = length;
        }

        private void EnsureSize()
        {
            if (_count >= _length)
            {
                Resize(_length * 2);
            }
        }

        private void ThrowIfOutOfRange(int index)
        {
            if (0 > index || index >= _count)
            {
                throw new IndexOutOfRangeException(index.ToString());
            }
        }

        private void ThrowIfNotFixed()
        {
            if (!_isFixedSize)
            {
                throw new InvalidOperationException("Memory block should be fixed to get reference");
            }
        }

        private void ThrowIfNotAvailable()
        {
            if (State == UnmanagedState.Available)
            {
                return;
            }

            throw new UnmanagedObjectUnavailable("Object has been disposed or heap is unavailable");
        }

        public struct Enumerator:IEnumerator<TValue>
        {
            private readonly uint _version;
            private readonly Managed<TValue> _parent;

            private int _index;

            public Enumerator(Managed<TValue> parent) : this()
            {
                _index = -1;
                _version = parent._version;
                _parent = parent;
            }

            public bool MoveNext()
            {
                if (_version != _parent._version)
                {
                    throw new InvalidOperationException("The collection was modified after the enumerator was created");
                }

                _index++;
                return _parent.Count > _index;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public TValue Current => _parent._ptr[_index];

            object IEnumerator.Current => throw new NotSupportedException();

            public void Dispose()
            {
            }
        }
    }
}