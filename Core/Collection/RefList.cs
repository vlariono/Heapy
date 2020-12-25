using System;
using System.Collections.Generic;
using Heapy.Core.Enum;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Collection
{
    public unsafe ref struct RefList<TItem> where TItem:unmanaged
    {
        private static readonly int DefaultCapacity = 4;
        private readonly IUnmanagedHeap _heap;
        private int _length;
        private int _count;

        private TItem* _ptr;
        private uint* _version;

        public RefList(int length) : this()
        {
            _count = 0;
            _length = Math.Max(length,DefaultCapacity);
            _heap = UnmanagedHeap.Heap.Current;

            _ptr = _heap.Alloc<TItem>((uint) (sizeof(TItem) * _length));
            _version = _heap.Alloc<uint>(value:0);
        }

        public UnmanagedState State => (IntPtr) _ptr == IntPtr.Zero || Heap.State != UnmanagedState.Available
            ? UnmanagedState.Unavailable 
            : UnmanagedState.Available;

        public IUnmanagedHeap Heap => _heap;

        public void Dispose()
        {
            if ((IntPtr) _ptr != IntPtr.Zero)
            {
                _heap.Free((IntPtr)_ptr);
                _ptr = (TItem*)IntPtr.Zero;
            }

            if ((IntPtr) _version != IntPtr.Zero)
            {
                _heap.Free((IntPtr) _version);
                _version = (uint*) IntPtr.Zero;
            }
        }

        public ref TItem this[int index]
        {
            get
            {
                ThrowIfOutOfRange(index);
                return ref _ptr[index];
            }
        }

        public void Add(TItem item)
        {
            SetCapacity();
            Version++;

            _ptr[_count++] = item;
        }

        public void AddRange(ReadOnlySpan<TItem> items)
        {

        }

        public void RemoveAt(int index)
        {
            ThrowIfOutOfRange(index,true,nameof(index));
            Version++;

            var span = new Span<TItem>(_ptr, _count);
            var sourceSpan = span.Slice(index + 1, _count - index -1);
            var destinationSpan = span.Slice(index, _count - index);
            sourceSpan.CopyTo(destinationSpan);
            span[_count--] = default;
        }

        public Enumerator GetEnumerator()
        {
            return new(this);
        }

        public TItem[] ToArray()
        {
            return new Span<TItem>(_ptr, _length).ToArray();
        }

        public RefArray<TItem> ToRefArray()
        {
            return new(new ReadOnlySpan<TItem>(_ptr, _count));
        }

        public ref struct Enumerator
        {
            private readonly RefList<TItem> _parent;
            private readonly uint _version;

            private int _index;

            public Enumerator(RefList<TItem> parent) : this()
            {
                _index = -1;
                _parent = parent;
                _version = _parent.Version;
            }

            public ref TItem Current => ref _parent[_index];

            public bool MoveNext()
            {
                if (_parent.Version != _version)
                {
                    throw new InvalidOperationException(
                        "The collection was modified after the enumerator was created.");
                }

                _index++;
                return _index < _parent._count;
            }
        }

        private int RequiredMaxCapacity => Math.Max(DefaultCapacity,_count*2);
        private ref uint Version => ref *_version;

        private void SetCapacity()
        {
            if (_count >=_length)
            {
                _length = RequiredMaxCapacity;
                var newPtr = _heap.Realloc<TItem>((IntPtr) _ptr, (uint)(RequiredMaxCapacity*sizeof(TItem)));
                _ptr = newPtr;
            }
        }

        private void ThrowIfOutOfRange(int index, bool argument = false, string argumentName = "")
        {
            if (0 > index && index >= _count)
            {
                if (argument)
                {
                    throw new ArgumentOutOfRangeException($"{argumentName}");
                }
                throw new IndexOutOfRangeException(index.ToString());
            }
        }
    }
}
