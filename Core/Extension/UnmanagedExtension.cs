using System;
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

        /// <summary>
        /// Removes the element at the specified index
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="unmanagedValue">Instance of <see cref="Unmanaged{TValue}"/></param>
        /// <param name="index">The zero-based index of the element to remove</param>
        public static void RemoveAt<TValue>(this Unmanaged<TValue> unmanagedValue, int index) where TValue : unmanaged
        {
            if (index == unmanagedValue.Length - 1)
            {
                unmanagedValue[index] = default;
                return;
            }
            var span = unmanagedValue.AsSpan();
            var sourceSpan = span.Slice(index + 1, span.Length - index - 1);
            var destinationSpan = span.Slice(index, span.Length - index);
            sourceSpan.CopyTo(destinationSpan);
            span[^1] = default;
        }

        /// <summary>
        /// Copies from <see cref="Span{T}"/> to <see cref="Unmanaged{TValue}"/>
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="unmanagedValue">Instance of <see cref="Unmanaged{TValue}"/></param>
        /// <param name="span">Source <see cref="Span{T}"/></param>
        public static void CopyFrom<TValue>(this Unmanaged<TValue> unmanagedValue, ReadOnlySpan<TValue> span) where TValue : unmanaged
        {
            span.CopyTo(unmanagedValue);
        }

        /// <summary>
        /// Copies from <see cref="Unmanaged{TValue}"/> to <see cref="Span{T}"/>
        /// </summary>
        /// <typeparam name="TValue">The type of items in unmanaged memory</typeparam>
        /// <param name="unmanagedValue">Instance of <see cref="Unmanaged{TValue}"/></param>
        /// <param name="span">Destination <see cref="Span{T}"/></param>
        public static void CopyTo<TValue>(this Unmanaged<TValue> unmanagedValue, Span<TValue> span) where TValue : unmanaged
        {
            unmanagedValue.AsSpan().CopyTo(span);
        }

        public static Span<TValue>.Enumerator GetEnumerator<TValue>(this Unmanaged<TValue> unmanagedValue) where TValue : unmanaged
        {
            return unmanagedValue.AsSpan().GetEnumerator();
        }
    }
}