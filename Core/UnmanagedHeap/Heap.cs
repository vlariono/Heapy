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
        /// Default heap will be used if no private heap is set
        /// </summary>
        public static IUnmanagedHeap Default => DefaultHeap;
        
        /// <summary>
        /// The heap is set either with <see cref="GetPrivateHeap"/> or with <see cref="SetPrivateHeap"/>
        /// </summary>
        public static IUnmanagedHeap? Private => LocalHeap.Value?.State == UnmanagedState.Available ? LocalHeap.Value : null;
        /// <summary>
        /// The heap is <see cref="Private"/> if one is set or <see cref="Default"/>
        /// </summary>
        public static IUnmanagedHeap Current => Private ?? Default ?? throw new UnmanagedHeapUnavailable("Run init before using heap");

        /// <summary>
        /// Gets new instance of private heap
        /// </summary>
        /// <returns>Instance of heap from heap factory</returns>
        public static IUnmanagedHeap GetPrivateHeap()
        {
            LocalHeap.Value = HeapFactory.Value?.Invoke() ?? throw new UnmanagedHeapUnavailable("Failed to create heap or heap factory is not set");
            return LocalHeap.Value;
        }

        /// <summary>
        /// Sets heap factory to produce private heaps with <see cref="GetPrivateHeap"/>
        /// </summary>
        /// <param name="heapFactory">Factory method</param>
        public static void SetPrivateHeapFactory(Func<IUnmanagedHeap> heapFactory)
        {
            HeapFactory.Value = heapFactory ?? throw new ArgumentNullException(nameof(heapFactory));
        }

        /// <summary>
        /// Sets instance of local heap to specified instance
        /// </summary>
        /// <param name="heap">Instance of heap</param>
        /// <returns>Instance of heap</returns>
        public static IUnmanagedHeap SetPrivateHeap(IUnmanagedHeap heap)
        {
            LocalHeap.Value = heap ?? throw new ArgumentNullException(nameof(heap));
            return LocalHeap.Value;
        }

        /// <summary>
        /// Allocates unmanaged memory from <see cref="Current"/> heap.
        /// </summary>
        /// <typeparam name="TValue">Type to map allocated memory to</typeparam>
        /// <returns>Pointer to unmanaged memory</returns>
        public static Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            return Current.Alloc<TValue>();
        }

        /// <summary>
        /// Allocates unmanaged memory from <see cref="Current"/> heap and copies value from <see cref="value"/>
        /// </summary>
        /// <typeparam name="TValue">Type to map allocated memory to</typeparam>
        /// <param name="value">Value to copy to allocated memory</param>
        /// <returns>Pointer to unmanaged memory</returns>
        public static Unmanaged<TValue> Alloc<TValue>(TValue value) where TValue : unmanaged
        {
            return Current.Alloc<TValue>(value);
        }

        /// <summary>
        /// Allocates unmanaged memory of specified size <see cref="bytes"/>
        /// </summary>
        /// <typeparam name="TValue">Type to map allocated memory to</typeparam>
        /// <param name="bytes">Amount of memory to allocate</param>
        /// <returns>Pointer to unmanaged memory</returns>
        public static Unmanaged<TValue> Alloc<TValue>(uint bytes) where TValue : unmanaged
        {
            return Current.Alloc<TValue>(bytes);
        }

        /// <summary>
        /// Allocates unmanaged memory of specified size <see cref="bytes"/> with options 
        /// </summary>
        /// <typeparam name="TValue">Type to map allocated memory to</typeparam>
        /// <param name="bytes">Amount of memory to allocate</param>
        /// <param name="options">Platform specific allocation options</param>
        /// <returns>Pointer to unmanaged memory</returns>
        public static Unmanaged<TValue> Alloc<TValue>(uint bytes, uint options) where TValue : unmanaged
        {
            return Current.Alloc<TValue>(bytes, options);
        }

        /// <summary>
        /// Reallocates memory to new size
        /// </summary>
        /// <typeparam name="TValue">Type to map allocated memory to</typeparam>
        /// <param name="memory">Pointer to allocated memory</param>
        /// <param name="bytes">New amount of memory to allocate</param>
        /// <returns>Pointer to unmanaged memory</returns>
        public static Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes) where TValue : unmanaged
        {
            return Current.Realloc<TValue>(memory, bytes);
        }

        /// <summary>
        /// Reallocates memory to new size with options
        /// </summary>
        /// <typeparam name="TValue">Type to map allocated memory to</typeparam>
        /// <param name="memory">Pointer to allocated memory</param>
        /// <param name="bytes">New amount of memory to allocate</param>
        /// <param name="options">Platform specific allocation options</param>
        /// <returns>Pointer to unmanaged memory</returns>
        public static Unmanaged<TValue> Realloc<TValue>(IntPtr memory, uint bytes, uint options) where TValue : unmanaged
        {
            return Current.Realloc<TValue>(memory, bytes, options);
        }

        /// <summary>
        /// Deallocate memory
        /// </summary>
        /// <param name="memory">Memory to deallocate</param>
        /// <returns>true - succeeded and false - in case of error</returns>
        public static bool Free(IntPtr memory)
        {
            return Current.Free(memory);
        }
    }
}