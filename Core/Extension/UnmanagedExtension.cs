using System;
using System.Runtime.CompilerServices;
using Heapy.Core.UnmanagedHeap;

namespace Heapy.Core.Extension
{
    public static class UnmanagedExtension
    {
        /// <summary>
        /// Converts managed <see cref="value"/> to <see cref="Unmanaged{TValue}"/>
        /// </summary>
        /// <typeparam name="TValue">Type of the item</typeparam>
        /// <param name="value">Value to copy to unmanaged memory</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> ToUnmanaged<TValue>(this TValue value) where TValue : unmanaged
        {
            return Heap.Alloc(value);
        }

        /// <summary>
        /// Converts managed <see cref="values"/> to <see cref="Unmanaged{TValue}"/>
        /// </summary>
        /// <typeparam name="TValue">Type of the items</typeparam>
        /// <param name="values">Values to copy to managed memory</param>
        /// <returns><see cref="Unmanaged{TValue}"/></returns>
        public static Unmanaged<TValue> ToUnmanaged<TValue>(this TValue[] values) where TValue : unmanaged
        {
            var unmanagedValue = Heap.Alloc<TValue>(values.Length);
            values.CopyTo(unmanagedValue);
            return unmanagedValue;
        }

        public static Managed<TValue> ToManaged<TValue>(this Unmanaged<TValue> unmanagedValue, bool preserveExisting = false) where TValue:unmanaged
        {
            return new(unmanagedValue,preserveExisting);
        }

        public static unsafe bool Equals<TValue>(this TValue firstValue, TValue secondValue) where TValue : unmanaged
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