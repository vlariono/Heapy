using System;
using System.Threading;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    public static class Heap
    {
        private static readonly IUnmanagedHeap DefaultHeap;
        private static readonly AsyncLocal<IUnmanagedHeap?> LocalHeap;
        private static readonly AsyncLocal<Func<IUnmanagedHeap>> HeapFactory;

        static Heap()
        {
            DefaultHeap = ProcessHeap.Create();
            LocalHeap = new AsyncLocal<IUnmanagedHeap?>();
            HeapFactory = new AsyncLocal<Func<IUnmanagedHeap>>();
        }

        /// <summary>
        /// Returns instance of default heap <see cref="ProcessHeap"/>
        /// </summary>
        public static IUnmanagedHeap Default => DefaultHeap;
        
        /// <summary>
        /// Returns current instance of private heap
        /// </summary>
        public static IUnmanagedHeap? Private => LocalHeap.Value?.State == UnmanagedState.Available ? LocalHeap.Value : null;
        /// <summary>
        /// Returns instance of current heap.
        /// Current heap is either <see cref="Private"/> or <see cref="Default"/>
        /// </summary>
        public static IUnmanagedHeap Current => Private ?? Default ?? throw new UnmanagedHeapUnavailable("Run init before using heap");

        /// <summary>
        /// Returns new instance of private heap.
        /// <see cref="Private"/> will return this instance for all subsequent calls till until it's disposed
        /// <see cref="Current"/> will return this instance for all subsequent calls till until it's disposed
        /// </summary>
        /// <returns><see cref="IUnmanagedHeap"/></returns>
        public static IUnmanagedHeap GetPrivateHeap()
        {
            LocalHeap.Value = HeapFactory.Value?.Invoke() ?? throw new UnmanagedHeapUnavailable("Failed to create heap or heap factory is not set");
            return LocalHeap.Value;
        }

        /// <summary>
        /// Sets private heap factory to produce <see cref="IUnmanagedHeap"/> instance
        /// <see cref="GetPrivateHeap"/> uses this factory to create new instance of private heap
        /// </summary>
        /// <param name="heapFactory"><see cref="Func{TResult}"/></param>
        public static void SetPrivateHeapFactory(Func<IUnmanagedHeap> heapFactory)
        {
            HeapFactory.Value = heapFactory ?? throw new ArgumentNullException(nameof(heapFactory));
        }

        /// <summary>
        /// Sets instance of private heap to specified instance
        /// <see cref="Private"/> will return this instance for all subsequent calls till until it's disposed
        /// <see cref="Current"/> will return this instance for all subsequent calls till until it's disposed
        /// </summary>
        /// <param name="heap"><see cref="IUnmanagedHeap"/></param>
        /// <returns><see cref="IUnmanagedHeap"/></returns>
        public static IUnmanagedHeap SetPrivateHeap(IUnmanagedHeap heap)
        {
            LocalHeap.Value = heap ?? throw new ArgumentNullException(nameof(heap));
            return LocalHeap.Value;
        }

        /// <summary>
        /// Allocates a block of memory from <see cref="Current"/> heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged => Current.Alloc<TValue>();

        /// <summary>
        /// Allocates a block of memory from <see cref="Current"/> heap and copies <see cref="value"/> to allocated memory block. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="value">Value to copy to allocated memory</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged => Current.Alloc<TValue>(value);

        /// <summary>
        /// Allocates a block of memory from <see cref="Current"/> heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="length">Length of contiguous memory block</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> Alloc<TValue>(int length) where TValue : unmanaged => Current.Alloc<TValue>(length);

        /// <summary>
        /// Allocates a block of memory from <see cref="Current"/> heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="length">Length of contiguous memory block</param>
        /// <param name="options">Platform specific allocation options</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> Alloc<TValue>(int length, uint options) where TValue : unmanaged => Current.Alloc<TValue>(length, options);
    }
}