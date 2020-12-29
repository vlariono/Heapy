using System;
using System.Runtime.CompilerServices;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Extension
{
    public static class UnmanagedExtension
    {
        /// <summary>
        /// Determines whether two values are equal
        /// This method doesn't depends on <see cref="IEquatable{T}"/>
        /// </summary>
        /// <typeparam name="TValue">Type of value item</typeparam>
        /// <param name="firstValue">First value</param>
        /// <param name="secondValue">The value to compare with first value</param>
        /// <returns></returns>
        public static unsafe bool EqualsByValue<TValue>(this TValue firstValue, TValue secondValue) where TValue : unmanaged
        {
            var valueSize = sizeof(TValue);
            if (valueSize <= 8)
            {
                ref var first = ref Unsafe.As<TValue,ulong>(ref firstValue);
                ref var second = ref Unsafe.As<TValue,ulong>(ref secondValue);

                return first == second;
            }

            var sourceSpan = new Span<byte>(&firstValue, valueSize);
            var otherSpan = new Span<byte>(&secondValue, valueSize);
            return sourceSpan.SequenceEqual(otherSpan);
        }
    }
}