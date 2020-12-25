﻿using System;
using Heapy.Core.Enum;
using Heapy.Core.Interface;

namespace Heapy.Core.Collection
{
    public unsafe ref struct RefArray<TItem> where TItem : unmanaged
    {
        private readonly Span<TItem> _span;
        private TItem* _ptr;

        public RefArray(IntPtr ptr, int length, IUnmanagedHeap heap) : this()
        {
            if (heap == null)
            {
                throw new ArgumentNullException(nameof(heap));
            }

            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            _ptr = (TItem*) ptr;
            Heap = heap;
            _span = new Span<TItem>(_ptr, length);
        }

        public RefArray(int length) : this()
        {
            Heap = UnmanagedHeap.Heap.Current;
            _ptr = Heap.Alloc<TItem>((uint) (sizeof(TItem) * length));
            _span = new Span<TItem>(_ptr, length);
        }

        public RefArray(ReadOnlySpan<TItem> items)
        {
            Heap = UnmanagedHeap.Heap.Current;
            _ptr = Heap.Alloc<TItem>((uint) (sizeof(TItem) * items.Length));
            _span = new Span<TItem>(_ptr, items.Length);
            items.CopyTo(_span);
        }

        public void Dispose()
        {
            if (State == UnmanagedState.Available)
            {
                Heap.Free(this);
                _ptr = (TItem*) IntPtr.Zero;
            }
        }

        public UnmanagedState State => (IntPtr) _ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
            ? UnmanagedState.Unavailable
            : UnmanagedState.Available;

        public IUnmanagedHeap Heap { get; }

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
        ///     Creates managed array
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

        public static implicit operator IntPtr(RefArray<TItem> array) => (IntPtr)array._ptr;
        public static implicit operator TItem*(RefArray<TItem> array) => array._ptr;
        public static implicit operator Span<TItem>(RefArray<TItem> array) => array._span;
    }
}