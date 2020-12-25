using System;
using Heapy.Core.Enum;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Collection
{
    public ref struct RefArray<TItem> where TItem : unmanaged
    {
        private readonly Span<TItem> _span;
        private Unmanaged<TItem> _array;

        public unsafe RefArray(IntPtr ptr, int length, IUnmanagedHeap heap) : this()
        {
            if (heap == null)
            {
                throw new ArgumentNullException(nameof(heap));
            }

            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            _array = new Unmanaged<TItem>(ptr, heap);
            _span = new Span<TItem>(_array, length);

        }

        public unsafe RefArray(int length) : this()
        {
            _array = Heap.Alloc<TItem>((uint) (sizeof(TItem) * length));
            _span = new Span<TItem>(_array, length);
        }

        public unsafe RefArray(ReadOnlySpan<TItem> items)
        {
            _array = Heap.Alloc<TItem>((uint) (sizeof(TItem) * items.Length));
            _span = new Span<TItem>(_array, items.Length);
            items.CopyTo(_span);
        }

        public void Dispose()
        {
            _array.Dispose();
        }

        public UnmanagedState State => _array.State;

        /// <summary>
        ///     Length of the array
        /// </summary>
        public int Length => _span.Length;

        public ref TItem this[int index] => ref _span[index];

        public void RemoveAt(int index)
        {
            var last = _span.Length - 1;
            var sourceSpan = _span.Slice(index + 1, last - index);
            var destinationSpan = _span.Slice(index, _span.Length - index);
            sourceSpan.CopyTo(destinationSpan);
            _span[last] = default;
        }

        public Span<TItem> AsSpan()
        {
            return _span;
        }

        /// <summary>
        ///     Create managed array
        /// </summary>
        /// <returns></returns>
        public TItem[] ToArray()
        {
            return _span.ToArray();
        }

        public Span<TItem>.Enumerator GetEnumerator() => _span.GetEnumerator();

        public override bool Equals(object? obj)
        {
            throw new NotSupportedException("Equals(object? obj)");
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException("GetHashCode()");
        }

        public static implicit operator IntPtr(RefArray<TItem> array) => array._array;
        public static unsafe implicit operator TItem*(RefArray<TItem> array) => array._array;
        public static implicit operator Span<TItem>(RefArray<TItem> array) => array._span;
    }
}