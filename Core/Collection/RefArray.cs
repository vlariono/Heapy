using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Collection
{
    public unsafe ref struct RefArray<TItem> where TItem : unmanaged
    {
        private readonly IUnmanagedHeap _heap;
        private readonly int _length;
        
        private TItem* _ptr;

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
            _length = length;
            _heap = heap;
        }

        public RefArray(int length) : this()
        {
            _length = length;
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
            _heap = UnmanagedHeap.Heap.Current;

            _ptr = Heap.Alloc<TItem>((uint)(sizeof(TItem) * _length));

            var spanSource = new Span<TItem>(items);
            var spanDestination = new Span<TItem>(_ptr, items.Length);
            spanSource.CopyTo(spanDestination);
        }

        public UnmanagedState State => (IntPtr)_ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
                                ? UnmanagedState.Unavailable
                                : UnmanagedState.Available;

        public IUnmanagedHeap Heap => _heap;

        /// <summary>
        /// Length of the array
        /// </summary>
        public int Length => _length;

        public TItem this[int index]
        {
            get
            {
                ThrowIfUnavailable();
                if (0 > index || index >= _length)
                {
                    throw new IndexOutOfRangeException(index.ToString());
                }

                return _ptr[index];
            }
        }

        public void RemoveAt(int index)
        {
            ThrowIfUnavailable();
            if (0 > index || index >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var last = _length - 1;
            var sourceSpan = new Span<TItem>(_ptr, _length).Slice(index + 1, last - index );
            var destinationSpan = new Span<TItem>(_ptr, _length).Slice(index, _length - index);
            sourceSpan.CopyTo(destinationSpan);
            _ptr[last] = default;
        }

        public Span<TItem> AsSpan()
        {
            ThrowIfUnavailable();
            return new(_ptr, _length);
        }

        /// <summary>
        /// Creates managed array
        /// </summary>
        /// <returns></returns>
        public TItem[] ToArray()
        {
            ThrowIfUnavailable();
            return AsSpan().ToArray();
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                Heap.Free(this);
                _ptr = (TItem*)IntPtr.Zero;
            }
        }
        
        private void ThrowIfUnavailable()
        {
            if (State != UnmanagedState.Available)
            {
                throw new UnmanagedObjectUnavailable($"{nameof(RefArray<TItem>)} is unavailable");
            }
        }

        public static implicit operator IntPtr(RefArray<TItem> array) => (IntPtr)array._ptr;
        public static implicit operator TItem*(RefArray<TItem> array) => array._ptr;
    }
}