using System;
using Heapy.Core.Exceptions;
using Heapy.Core.UnmanagedHeap;
using Xunit;

namespace Tests.UnmanagedHeap
{
    public class UnmanagedTests
    {
        private readonly UnmanagedTest[] _array;

        private struct UnmanagedTest
        {
            public int I1 { get; set; }
            public int I2 { get; set; }
        }

        public UnmanagedTests()
        {
            _array = new UnmanagedTest[]
            {
                new()
                {
                    I1 = 10,
                    I2 = 20
                },
                new()
                {
                    I1 = 12,
                    I2 = 22
                },
                new()
                {
                    I1 = 13,
                    I2 = 23
                }
            };
        }

        [Fact]
        public unsafe void UnmanagedProperties_IsCorrect()
        {
            using var mem = Unmanaged<UnmanagedTest>.Alloc(50);
            Assert.NotEqual(IntPtr.Zero,mem.Ptr);
            Assert.NotEqual(IntPtr.Zero,mem);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void UnmanagedIndexer_ReturnsValue(int index)
        {
            using var mem = Unmanaged<UnmanagedTest>.Alloc(_array.Length);
            mem[index] = _array[index];
            Assert.Equal(_array[index],mem[index]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        public void UnmanagedIndexer_ThrowsOurOfRange(int index)
        {
            using var mem = Unmanaged<UnmanagedTest>.Alloc(2);
            try
            {
                var m = mem[index];
            }
            catch (Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }
        }

        [Fact]
        public void UnmanagedIndexer_AllowsToChangeProperty()
        {
            using var mem = Unmanaged<UnmanagedTest>.Alloc(2);
            mem[0].I1 = 10;
            mem[1].I2 = 21;
            Assert.Equal(10,mem[0].I1);
            Assert.Equal(21,mem[1].I2);
        }

        [Fact]
        public void RemoveAt_ThrowsObjectUnavailable()
        {
            var mem = new Unmanaged<UnmanagedTest>();
            mem.Dispose();
            try
            {
                var temp = mem.Ptr;
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }

            try
            {
                var temp = mem.Length;
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }

            try
            {
                var temp = mem[0];
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }

            try
            {
                var temp = mem.AsSpan();
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }
        }
    }
}