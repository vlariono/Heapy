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
        private int _free;

        public RefArray(int length) : this()
        {
            _length = length;
            _free = length;
            _heap = HeapManager.Current;

            _ptr = (TItem*)(IntPtr)Heap.Alloc<TItem>((uint)(sizeof(TItem) * length));
        }

        public IntPtr Ptr
        {
            private get => (IntPtr)_ptr;
            init => _ptr = (TItem*)value;
        }

        public TItem Last => _ptr[LastIndex];
        public int LastIndex => Length - Free;

        public UnmanagedState State => _disposed || Ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        public IUnmanagedHeap Heap
        {
            get => _heap;
            init => _heap = value;
        }

        public int Length
        {
            get => _length;
            init => _length = value;
        }

        public int Free => _free;

        public ref TItem this[int index]
        {
            get
            {
                if (0 > index || index > _length - _free)
                {
                    throw new IndexOutOfRangeException();
                }

                return ref _ptr[index];
            }
        }

        public void Add(TItem item)
        {
            if (LastIndex >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(item), "Array is full");
            }

            _ptr[LastIndex] = item;
            _free--;
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