using System;
using System.Threading;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.Heap
{
    public static class HeapManager
    {
        private static readonly object Lock = new();
        
        private static Func<IUnmanagedHeap>? _heapFactory;
        private static Lazy<IUnmanagedHeap>? _defaultHeap;
        private static AsyncLocal<IUnmanagedHeap?>? _localHeap;

        private static bool _initialized;

        /// <summary>
        /// Initializes heap factory.
        /// </summary>
        /// <param name="heapFactory">Heap factory</param>
        /// <returns>True if succeeded or False if factory has been initialized before</returns>
        public static bool Init(Func<IUnmanagedHeap> heapFactory)
        {
            lock (Lock)
            {
                if (_initialized)
                {
                    return false;
                }
                
                _heapFactory = heapFactory ?? throw new ArgumentNullException(nameof(heapFactory));
                _localHeap = new AsyncLocal<IUnmanagedHeap?>();
                _defaultHeap = new Lazy<IUnmanagedHeap>(heapFactory);
                _initialized = true;
                return true;
            }
        }

        public static IUnmanagedHeap Default => _defaultHeap?.Value ?? throw new UnmanagedHeapUnavailable("Run init before using heap");
        private static IUnmanagedHeap? Local => _localHeap?.Value?.State == UnmanagedState.Available ? _localHeap.Value : null;
        public static IUnmanagedHeap Current => Local ?? Default ?? throw new UnmanagedHeapUnavailable("Run init before using heap");

        /// <summary>
        /// Gets new instance of local heap
        /// </summary>
        /// <returns>Instance of heap from heap factory</returns>
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

        /// <summary>
        /// Set instance of local heap to specified instance
        /// </summary>
        /// <param name="heap"></param>
        /// <returns>Instance of heap</returns>
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