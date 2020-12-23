using System;
using System.Collections;
using System.Collections.Generic;

namespace Heapy.Core.Collection
{
    public unsafe struct UnmanagedEnumerator<TItem> : IEnumerator<TItem> where TItem:unmanaged
    {
        private readonly TItem* _array;
        private readonly int _count;
        private int _index;

        public UnmanagedEnumerator(TItem* array, int count) : this()
        {
            _index = -1;
            _array = array;
            _count = count;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }

        public void Reset()
        {
            _index = -1;
        }

        object IEnumerator.Current => throw new InvalidCastException("Cast to object is not supported");

        public TItem Current
        {
            get
            {
                try
                {
                    return _array[_index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}