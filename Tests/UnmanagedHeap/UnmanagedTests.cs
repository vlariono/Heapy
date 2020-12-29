using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;
using Xunit;

namespace Tests.UnmanagedHeap
{
    public class UnmanagedTests:IDisposable
    {
        private readonly IUnmanagedHeap _heap;
        private readonly UnmanagedTest[] _array;

        private struct UnmanagedTest
        {
            public int I1 { get; set; }
            public int I2 { get; set; }
        }

        public UnmanagedTests()
        {
            _heap = ProcessHeap.Create();
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

        public void Dispose()
        {
            _heap.Dispose();
        }

        [Fact]
        public void UnmanagedState_IsCorrect()
        {
            var mem = _heap.Alloc<UnmanagedTest>();
            Assert.Equal(UnmanagedState.Available,mem.State);
            mem.Dispose();
            Assert.Equal(UnmanagedState.Unavailable,mem.State);
        }

        [Fact]
        public unsafe void UnmanagedProperties_IsCorrect()
        {
            using var mem = _heap.Alloc<UnmanagedTest>(50);
            Assert.Equal(50,mem.Length);
            Assert.NotEqual(IntPtr.Zero,mem.Ptr);
            Assert.NotEqual(IntPtr.Zero,mem);
            Assert.False((UnmanagedTest*)IntPtr.Zero == mem);
            Assert.Equal(_heap,mem.Heap);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void UnmanagedIndexer_ReturnsValue(int index)
        {
            using var mem = _heap.Alloc<UnmanagedTest>(_array.Length);
            mem[index] = _array[index];
            Assert.Equal(_array[index],mem[index]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        public void UnmanagedIndexer_ThrowsOurOfRange(int index)
        {
            using var mem = _heap.Alloc<UnmanagedTest>(2);
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
            using var mem = _heap.Alloc<UnmanagedTest>(2);
            mem[0].I1 = 10;
            mem[1].I2 = 21;
            Assert.Equal(10,mem[0].I1);
            Assert.Equal(21,mem[1].I2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void IndexOf_ReturnsCorrectIndex(int index)
        {
            using var mem = _heap.Alloc<UnmanagedTest>(_array.Length);
            _array.AsSpan().CopyTo(mem);
            var resultIndex = mem.IndexOf(_array[index]);
            Assert.Equal(index,resultIndex);

            var indexOfDefault = mem.IndexOf(default);
            Assert.Equal(-1,indexOfDefault);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void RemoveAt_RemovesCorrectly(int index)
        {
            using var mem = _heap.Alloc<UnmanagedTest>(_array.Length);
            _array.AsSpan().CopyTo(mem);
            var requiredValue = index < _array.Length - 1 ? _array[index + 1] : default;
            mem.RemoveAt(index);
            var currentValue = mem[index];
            Assert.Equal(requiredValue,currentValue);
        }

        [Fact]
        public void RemoveAt_ThrowsObjectUnavailable()
        {
            var mem = _heap.Alloc<UnmanagedTest>();
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

            try
            {
                var temp = mem.IndexOf(default);
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }

            try
            {
                mem.RemoveAt(0);
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }

            try
            {
                var array = new UnmanagedTest[2];
                mem.CopyTo(array);
            }
            catch (Exception e)
            {
                Assert.True(e is UnmanagedObjectUnavailable);
            }
        }
    }
}