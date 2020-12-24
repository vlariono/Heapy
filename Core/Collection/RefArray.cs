using System;
using Heapy.Core.Enum;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Collection
{
    public unsafe ref struct RefArray<TItem> where TItem : unmanaged
    {
        private readonly TItem* _ptr;
        private readonly IUnmanagedHeap _heap;
        private readonly int _length;

        private bool _disposed;
        private int _count;

        public RefArray(IntPtr ptr, int length, int count, IUnmanagedHeap heap):this()
        {
            if (heap == null)
            {
                throw new ArgumentNullException(nameof(heap));
            }

            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            if (count > length)
            {
                throw new ArgumentOutOfRangeException(nameof(count),"Count cannot exceed length");
            }

            _ptr = (TItem*)ptr;
            _count = count;
            _length = length;
            _heap = heap;
        }

        public RefArray(int length) : this()
        {
            _length = length;
            _count = 0;
            _heap = UnmanagedHeap.Heap.Current;

            _ptr = Heap.Alloc<TItem>((uint)(sizeof(TItem) * _length));
        }

        public RefArray(TItem[] items, int length) : this()
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _length = items.Length + length;
            _count = items.Length;
            _heap = UnmanagedHeap.Heap.Current;

            _ptr = Heap.Alloc<TItem>((uint)(sizeof(TItem) * _length));

            var spanSource = new Span<TItem>(items);
            var spanDestination = new Span<TItem>(_ptr, items.Length);
            spanSource.CopyTo(spanDestination);
        }

        public TItem Last => _ptr[_count - 1];

        public UnmanagedState State => _disposed || (IntPtr)_ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        public IUnmanagedHeap Heap => _heap;

        /// <summary>
        /// Number of items in the array
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Length of the array
        /// </summary>
        public int Length => _length;

        public ref TItem this[int index]
        {
            get
            {
                if (0 > index || index >= _count)
                {
                    throw new IndexOutOfRangeException(index.ToString());
                }

                return ref _ptr[index];
            }
        }

        public void Add(TItem item)
        {
            if (_count >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(item), "Array is full");
            }
            _ptr[_count] = item;
            _count++;
        }

        public void RemoveAt(int index)
        {
            if (0 > index || index >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var sourceSpan = new Span<TItem>(_ptr, _count).Slice(index + 1, _count - index - 1);
            var destinationSpan = new Span<TItem>(_ptr, _count).Slice(index, _count - index);
            sourceSpan.CopyTo(destinationSpan);
            _ptr[_count--] = default;
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                Heap.Free(this);
            }
            _disposed = true;
        }

        public static implicit operator IntPtr(RefArray<TItem> array) => (IntPtr)array._ptr;
        public static implicit operator TItem*(RefArray<TItem> array) => array._ptr;
    }
}