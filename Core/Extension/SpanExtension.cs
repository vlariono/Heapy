using System;
using System.Runtime.CompilerServices;

namespace Heapy.Core.Extension
{
    public static class SpanExtension
    {
        public static void RemoteAt<TValue>(this Span<TValue> span, int index) where TValue : unmanaged
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
    }
}