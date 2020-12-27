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

        public static Managed<TValue> ToManaged<TValue>(this Unmanaged<TValue> unmanagedValue) where TValue:unmanaged
        {
            return new(unmanagedValue);
        }
    }
}