using System;
using Heapy.Core.Enum;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Interface
{
    public interface IUnmanagedHeap : IDisposable
    {
        /// <summary>
        /// Returns state of the heap <see cref="UnmanagedState"/>
        /// </summary>
        UnmanagedState State { get; }

        /// <summary>
        /// Allocates a block of memory from a heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged;

        /// <summary>
        /// Allocates a block of memory from a heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="length">Length of contiguous memory block</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        Unmanaged<TValue> Alloc<TValue>(int length) where TValue : unmanaged;

        /// <summary>
        /// Allocates a block of memory from a heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="length">Length of contiguous memory block</param>
        /// <param name="options">Platform specific allocation options</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        Unmanaged<TValue> Alloc<TValue>(int length, uint options) where TValue : unmanaged;

        /// <summary>
        /// Reallocates a block of memory from a heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="memory">A pointer to the block of memory that the function reallocates</param>
        /// <param name="length">New length of contiguous memory block</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length) where TValue : unmanaged;

        /// <summary>
        /// Reallocates a block of memory from a heap. The allocated memory is not movable
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="memory">A pointer to the block of memory that the function reallocates</param>
        /// <param name="length">New length of contiguous memory block</param>
        /// <param name="options">Platform specific allocation options</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length, uint options) where TValue : unmanaged;

        /// <summary>
        /// Frees a memory block allocated from a heap
        /// </summary>
        /// <param name="memory">A pointer to the memory block to be freed. </param>
        /// <returns>If the function succeeds - true, otherwise - false</returns>
        bool Free(IntPtr memory);
    }
}