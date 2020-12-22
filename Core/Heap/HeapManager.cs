using System;
using System.Threading;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.Heap
{
    public static class HeapManager
    {
        private static Func<IUnmanagedHeap>? _heapFactory;
        private static Lazy<IUnmanagedHeap>? _defaultHeap;
        private static AsyncLocal<IUnmanagedHeap?>? _localHeap;

        public static void Init(Func<IUnmanagedHeap> heapFactory)
        {
            if (_heapFactory != null)
            {
                throw new ArgumentException($"{nameof(HeapManager)} has already been initialized");
            }
            _heapFactory = heapFactory ?? throw new ArgumentNullException(nameof(heapFactory));
            _localHeap = new AsyncLocal<IUnmanagedHeap?>();
            _defaultHeap = new Lazy<IUnmanagedHeap>(heapFactory);
        }

        public static IUnmanagedHeap Default => _defaultHeap?.Value ?? throw new UnmanagedHeapUnavailable("Run init before using heap");
        private static IUnmanagedHeap? Local => _localHeap?.Value?.State == UnmanagedState.Available ? _localHeap.Value : null;
        public static IUnmanagedHeap Current => Local ?? Default ?? throw new UnmanagedHeapUnavailable("Run init before using heap");

        public static IUnmanagedHeap GetLocalHeap()
        {
            if (_localHeap == null || _heapFactory == null)
            {
                throw new UnmanagedHeapUnavailable("Run init before using heap");
            }

            if (_localHeap.Value == null || _localHeap.Value.State != UnmanagedState.Available)
            {
                _localHeap.Value = _heapFactory.Invoke();
            }

            return _localHeap.Value;
        }

        public static IUnmanagedHeap SetLocalHeap(IUnmanagedHeap heap)
        {
            if (_localHeap == null)
            {
                throw new UnmanagedHeapUnavailable("Run init before using heap");
            }

            _localHeap.Value = heap ?? throw new ArgumentNullException(nameof(heap));
            return _localHeap.Value;
        }
    }
}