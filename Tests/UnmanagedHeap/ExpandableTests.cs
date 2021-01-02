using System;
using Heapy.Core.Extension;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;
using Xunit;

namespace Tests.UnmanagedHeap
{
    public class ExpandableTests
    {
        private readonly IUnmanagedHeap _heap;
        private readonly UnmanagedItem[] _testItems;
        private struct  UnmanagedItem
        {
            public int I1 { get; set; }
            public int I2 { get; set; }
            public long L1 { get; set; }
        }
        
        public ExpandableTests()
        {
            _testItems = new UnmanagedItem[20];
            for (var i = 0; i < _testItems.Length; i++)
            {
                _testItems[i] = new UnmanagedItem() {I1 = i + 10, I2 = i + 20, L1 = i + 30};
            }
            
            _heap = ProcessHeap.Create();
        }

        [Fact]
        public void AllocExpandable_OfLength()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>(5);
            Assert.Equal(5,items.Length);
            Assert.Equal(0,items.Count);
            Assert.Equal(_heap,items.Heap);
            Assert.False(items.IsFixed);
        }

        [Fact]
        public void Expandable_AddNewItem()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();

            foreach (var t in _testItems)
            {
                items.Add(t);
            }
            
            for (var i = 0; i < _testItems.Length; i++)
            {
                Assert.Equal(_testItems[i],items[i]);
            }
            
            Assert.Equal(_testItems.Length,items.Count);
            Assert.True(items.Length > items.Count);

            var item = items[10];
            Assert.Equal(10,items.IndexOf(item));
        }

        [Fact]
        public void Expandable_AddRange()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);
            Assert.Equal(_testItems.Length,items.Count);
            Assert.Equal(items.Length, items.Count);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void Expandable_IndexIsCorrect(int index)
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);
            Assert.Equal(_testItems[index], items[index]);
        }
        
        [Fact]
        public void Expandable_IndexIsOutOfRange()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);
            try
            {
                items.RemoveAt(-5);
            }
            catch (Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }
            
            try
            {
                items.RemoveAt(_testItems.Length+1);
            }
            catch (Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void Expandable_RemoveAt(int index)
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);

            try
            {
                items.RemoveAt(-5);
            }
            catch (Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }
            
            try
            {
                items.RemoveAt(_testItems.Length+1);
            }
            catch (Exception e)
            {
                Assert.True(e is IndexOutOfRangeException);
            }
            
            var item = items[index + 1];
            items.RemoveAt(index);
            Assert.Equal(item,items[index]);
        }
        
        [Fact]
        public void Expandable_RemoveAtLast()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);

            var maxIndex = items.Count - 1;
            items.RemoveAt(maxIndex);
            Assert.Equal(_testItems[items.Count-1],_testItems[items.Count-1]);
        }

        [Fact]
        public void Expandable_CopyTo()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);
            var copyItems = new UnmanagedItem[items.Count];
            items.CopyTo(copyItems);

            for (var i = 0; i < _testItems.Length; i++)
            {
                Assert.Equal(_testItems[i],copyItems[i]);
                Assert.Equal(_testItems[i],items[i]);
            }
        }

        [Fact]
        public void Expandable_AsSpan()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);
            var span = items.AsSpan();
            
            Assert.True(items.IsFixed);
            for (var i = 0; i < _testItems.Length; i++)
            {
                _testItems[i].Equals(span[i]);
            }
        }


        [Fact]
        public void Expandable_Ptr()
        {
            using var items = _heap.AllocExpandable<UnmanagedItem>();
            items.AddRange(_testItems);
            var ptr = items.Ptr;
            Assert.True(items.IsFixed);
            Assert.NotEqual(IntPtr.Zero,ptr);
        }
    }
}