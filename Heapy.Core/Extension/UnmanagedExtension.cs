using System;
using System.Runtime.CompilerServices;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Extension
{
    public static class UnmanagedExtension
    {
        private static ulong ToULong<TValue>(this TValue value) where TValue : unmanaged
        {
            return Unsafe.SizeOf<TValue>() switch
            {
                1 => Unsafe.As<TValue, byte>(ref value),
                2 => Unsafe.As<TValue, ushort>(ref value),
                4 => Unsafe.As<TValue, uint>(ref value),
                8 => Unsafe.As<TValue, ulong>(ref value),
                _ => throw new InvalidCastException()
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
            var valueSize = sizeof(TValue);
            if (valueSize <= sizeof(IntPtr))
            {
                var first = firstValue.ToULong();
                var second = secondValue.ToULong();

                return first == second;
            }

            var sourcePtr = Unsafe.AsPointer(ref firstValue);
            var destinationPtr = Unsafe.AsPointer(ref secondValue);
            var sourceSpan = new Span<byte>(sourcePtr, valueSize);
            var otherSpan = new Span<byte>(destinationPtr, valueSize);
            return sourceSpan.SequenceEqual(otherSpan);
        }

        /// <summary>
        /// Determines whether two sequences are equal
        /// </summary>
        /// <typeparam name="TValue">The type of elements in the sequence</typeparam>
        /// <param name="first">The first sequence to compare</param>
        /// <param name="second">The second sequence to compare</param>
        /// <returns><see cref="bool"/></returns>
        public static unsafe bool SequenceEqual<TValue>(this Unmanaged<TValue> first, Unmanaged<TValue> second) where TValue : unmanaged
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            if (first == second)
            {
                return true;
            }

            var length = sizeof(TValue) * first.Length;
            for (var i = 0; i < length; i++)
            {
                if (!EqualsByValue(ref first[i], ref second[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}