using System;
using Heapy.Core.Enum;
using Heapy.Core.Heap;
using Heapy.Core.Interface;

namespace Heapy.Core.Collection
{
    public unsafe ref struct RefArray<TItem> where TItem : unmanaged
    {
        private readonly TItem* _ptr;
        private readonly IUnmanagedHeap _heap;
        private readonly int _length;

        private bool _disposed;
        private int _count;

        public RefArray(int length) : this()
        {
            _length = length;
            _count = 0;
            _heap = HeapManager.Current;

            _ptr = (TItem*)(IntPtr)Heap.Alloc<TItem>((uint)(sizeof(TItem) * _length));
        }

        public RefArray(TItem[] items, int length) : this()
        {
            _length = items.Length + length;
            _count = items.Length;
            _heap = HeapManager.Current;

            _ptr = (TItem*)(IntPtr)Heap.Alloc<TItem>((uint)(sizeof(TItem) * _length));

            var spanSource = new Span<TItem>(items);
            var spanDestination = new Span<TItem>(_ptr, items.Length);
            spanSource.CopyTo(spanDestination);
        }

        public IntPtr Ptr
        {
            private get => (IntPtr)_ptr;
            init => _ptr = (TItem*)value;
        }

        public TItem Last => _ptr[_count - 1];

        public UnmanagedState State => _disposed || Ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        public IUnmanagedHeap Heap
        {
            get => _heap;
            init => _heap = value;
        }

        /// <summary>
        /// Number of items in the array
        /// </summary>
        public int Count
        {
            get => _count;
            init => _count = value;
        }

        /// <summary>
        /// Length of the array
        /// </summary>
        public int Length
        {
            get => _length;
            init => _length = value;
        }

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
                Heap.Free(Ptr);
            }
            _disposed = true;
        }
    }
}