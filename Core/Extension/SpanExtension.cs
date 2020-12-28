using System;
using System.Runtime.CompilerServices;

namespace Heapy.Core.Extension
{
    public static class SpanExtension
    {
        public static void RemoveAt<TValue>(this Span<TValue> span, int index) where TValue : unmanaged
        {
            if (0 > index || index >= span.Length)
            {
                throw new IndexOutOfRangeException(index.ToString());
            }

            if (index == span.Length - 1)
            {
                span[index] = default;
                return;
            }
            var sourceSpan = span.Slice(index + 1, span.Length - index - 1);
            var destinationSpan = span.Slice(index, span.Length - index);
            sourceSpan.CopyTo(destinationSpan);
            span[^1] = default;
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence
        /// The method doesn't depend on <see cref="IEquatable{T}"/>
        /// </summary>
        /// <typeparam name="TValue">The type of the span and value</typeparam>
        /// <param name="span">The span to search</param>
        /// <param name="item">The value to search for</param>
        /// <returns>If succeeded - index of item, otherwise -1</returns>
        public static int IndexOfByValue<TValue>(this ReadOnlySpan<TValue> span, TValue item) where TValue : unmanaged
        {
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i].EqualsByValue(item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}