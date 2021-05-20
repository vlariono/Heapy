using System;
using System.Runtime.CompilerServices;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Extension
{
    public static class UnmanagedExtension
    {
        private static unsafe int GetBlockSize(int bytesCount)
        {
            var ptrSize = sizeof(IntPtr);
            while (bytesCount % ptrSize != 0)
            {
                ptrSize >>= 1;
            }
            return ptrSize;
        }

        private static unsafe bool MemoryEquals<TValue>(IntPtr first, IntPtr second, int length) where TValue : unmanaged, IEquatable<TValue>
        {
            var sourceSpan = new Span<TValue>((void*)first, length);
            var otherSpan = new Span<TValue>((void*)second, length);
            return sourceSpan.SequenceEqual(otherSpan);
        }

        private static unsafe bool EqualsByValue<TValue>(IntPtr first, IntPtr second, int length) where TValue : unmanaged
        {
            var bytesCount = sizeof(TValue) * length;
            var blockSize = GetBlockSize(bytesCount);
            var memoryLength = bytesCount / blockSize;
            return blockSize switch
            {
                8 => MemoryEquals<ulong>(first, second, memoryLength),
                4 => MemoryEquals<uint>(first, second, memoryLength),
                2 => MemoryEquals<ushort>(first, second, memoryLength),
                1 => MemoryEquals<byte>(first, second, memoryLength),
                _ => throw new InvalidOperationException("Failed to compare memory blocks"),
            };
        }

        /// <summary>
        /// Determines whether two values are equal
        /// This method doesn't depends on <see cref="IEquatable{T}"/>
        /// </summary>
        /// <typeparam name="TValue">Type of value item</typeparam>
        /// <param name="firstValue">First value</param>
        /// <param name="secondValue">The value to compare with first value</param>
        /// <returns><see cref="bool"/></returns>
        public static unsafe bool EqualsByValue<TValue>(this ref TValue firstValue, ref TValue secondValue) where TValue : unmanaged
        {
            fixed (TValue* sourcePtr = &firstValue,destinationPtr = &secondValue)
            {
                return EqualsByValue<TValue>((IntPtr)sourcePtr, (IntPtr)destinationPtr, 1);
            }
        }

        /// <summary>
        /// Determines whether two sequences are equal
        /// </summary>
        /// <typeparam name="TValue">The type of elements in the sequence</typeparam>
        /// <param name="first">The first sequence to compare</param>
        /// <param name="second">The second sequence to compare</param>
        /// <returns><see cref="bool"/></returns>
        public static bool SequenceEqual<TValue>(this Unmanaged<TValue> first, Unmanaged<TValue> second) where TValue : unmanaged
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            if (first == second)
            {
                return true;
            }

            return EqualsByValue<TValue>(first, second, first.Length);
        }
    }
}