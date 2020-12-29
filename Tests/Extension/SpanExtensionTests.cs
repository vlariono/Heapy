using System;
using Heapy.Core.Extension;
using Xunit;

namespace Tests.Extension
{
    public class SpanExtensionTests
    {
        private readonly UnmanagedTest[] _array;

        private struct UnmanagedTest
        {
            public short S1 { get; set; }
            public int I1 { get; set; }
        };

        public SpanExtensionTests()
        {
            _array = new UnmanagedTest[]
            {
                new UnmanagedTest()
                {
                    I1 = 10,
                    S1 = 20
                },
                new UnmanagedTest()
                {
                    I1 = 11,
                    S1 = 21
                },
                new UnmanagedTest()
                {
                    I1 = 12,
                    S1 = 22
                },
                new UnmanagedTest()
                {
                    I1 = 13,
                    S1 = 23
                },
                new UnmanagedTest()
                {
                    I1 = 14,
                    S1 = 24
                }
            };
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void IndexOfByValue_ReturnsIndex(int index)
        {
            var roSpan = (ReadOnlySpan<UnmanagedTest>) _array.AsSpan();
            var indexResult = roSpan.IndexOfByValue(_array[index]);
            Assert.Equal(index,indexResult);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void IndexOfByValue_ThrowsOutOfRange(int index)
        {
            try
            {
                var roSpan = (ReadOnlySpan<UnmanagedTest>) _array.AsSpan();
                roSpan.IndexOfByValue(_array[index]);
            }
            catch(Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        public void RemoveAt_RemovesByIndex(int index)
        {
            var span = _array.AsSpan();
            var requiredValue = index < span.Length-1 ? span[index + 1]:default;
            span.RemoveAt(index);
            var currentValue = span[index];

            Assert.Equal(requiredValue,currentValue);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void RemoveAt_ThrowsOutOfRange(int index)
        {
            try
            {
                var span = _array.AsSpan();
                span.RemoveAt(index);
            }
            catch(Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }

        }
    }
}