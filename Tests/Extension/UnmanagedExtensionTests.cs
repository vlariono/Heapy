using Heapy.Core.Extension;
using Heapy.Core.UnmanagedHeap;
using Xunit;

namespace Tests.Extension
{
    public class UnmanagedExtensionTests
    {
        private struct UnmanagedTest1
        {
            public int I1 { get; set; }
            public int I2 { get; set; }
        }

        private struct UnmanagedTest2
        {
            public int I1 { get; set; }
            public long L2 { get; set; }
        }

        private struct UnmanagedTest3
        {
            public long L1 { get; set; }
            public long L2 { get; set; }
        }

        private unsafe struct UnmanagedTest4
        {
            public fixed byte Bytes[3];
            public long L1 { get; set; }
            public long L2 { get; set; }
        }

        private unsafe struct UnmanagedTest5
        {
            public short S1 { get; set; }
            public byte B2 { get; set; }
        }

        [Fact]
        public unsafe void EqualsByValue_Equals()
        {
            var value1 = new UnmanagedTest1() { I1 = 100, I2 = 200 };
            var value2 = new UnmanagedTest1() { I1 = 100, I2 = 200 };
            Assert.True(value1.EqualsByValue(ref value2));

            var value3 = new UnmanagedTest2() { I1 = 1000, L2 = 2000 };
            var value4 = new UnmanagedTest2() { I1 = 1000, L2 = 2000 };
            Assert.True(value3.EqualsByValue(ref value4));

            var value5 = new UnmanagedTest3() { L1 = 10000, L2 = 20000 };
            var value6 = new UnmanagedTest3() { L1 = 10000, L2 = 20000 };
            Assert.True(value5.EqualsByValue(ref value6));

            var value7 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            var value8 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            value7.Bytes[1] = 112;
            value8.Bytes[1] = 112;
            Assert.True(value7.EqualsByValue(ref value8));

            var value9 = new UnmanagedTest5() { S1 = 100, B2 = 15 };
            var value10 = new UnmanagedTest5() { S1 = 100, B2 = 15 };
            Assert.True(value9.EqualsByValue(ref value10));

            var valueArray = new UnmanagedTest5[] { new() { S1 = 100, B2 = 15 }, new() { S1 = 100, B2 = 15 } };
            Assert.True(valueArray[0].EqualsByValue(ref valueArray[1]));
        }

        [Fact]
        public unsafe void EqualsByValue_NotEquals()
        {
            var value1 = new UnmanagedTest1() { I1 = 150, I2 = 200 };
            var value2 = new UnmanagedTest1() { I1 = 100, I2 = 200 };
            Assert.False(value1.EqualsByValue(ref value2));

            var value3 = new UnmanagedTest2() { I1 = 1000, L2 = 2000 };
            var value4 = new UnmanagedTest2() { I1 = 1000, L2 = 2500 };
            Assert.False(value3.EqualsByValue(ref value4));

            var value5 = new UnmanagedTest3() { L1 = 10000, L2 = 20000 };
            var value6 = new UnmanagedTest3() { L1 = 10050, L2 = 20000 };
            Assert.False(value5.EqualsByValue(ref value6));

            var value7 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            var value8 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            value7.Bytes[0] = 112;
            value8.Bytes[1] = 112;
            Assert.False(value7.EqualsByValue(ref value8));

            var value9 = new UnmanagedTest5() { S1 = 150, B2 = 15 };
            var value10 = new UnmanagedTest5() { S1 = 100, B2 = 15 };
            Assert.False(value9.EqualsByValue(ref value10));
        }

        [Fact]
        public void SequenceEqual_Equals()
        {
            using var unmanagedValue1 = Unmanaged<UnmanagedTest1>.Alloc();
            var unmanagedValue2 = unmanagedValue1;
            Assert.True(unmanagedValue1.SequenceEqual(unmanagedValue2));

            using var unmanagedValue3 = Unmanaged<UnmanagedTest1>.Alloc();
            using var unmanagedValue4 = Unmanaged<UnmanagedTest1>.Alloc();
            unmanagedValue3[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            unmanagedValue4[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            Assert.True(unmanagedValue3.SequenceEqual(unmanagedValue4));

            using var unmanagedValue5 = Unmanaged<UnmanagedTest1>.Alloc(2);
            using var unmanagedValue6 = Unmanaged<UnmanagedTest1>.Alloc(2);
            unmanagedValue5[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            unmanagedValue5[1] = new UnmanagedTest1 { I1 = 101, I2 = 201 };
            unmanagedValue6[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            unmanagedValue6[1] = new UnmanagedTest1 { I1 = 101, I2 = 201 };
            Assert.True(unmanagedValue5.SequenceEqual(unmanagedValue6));

            using var unmanagedValue7 = Unmanaged<short>.Alloc();
            using var unmanagedValue8 = Unmanaged<short>.Alloc();
            unmanagedValue7[0] = 7;
            unmanagedValue8[0] = 7;
            Assert.True(unmanagedValue7.SequenceEqual(unmanagedValue8));
        }

        [Fact]
        public void SequenceEqual_NotEquals()
        {
            using var unmanagedValue1 = Unmanaged<UnmanagedTest1>.Alloc();
            using var unmanagedValue2 = Unmanaged<UnmanagedTest1>.Alloc(2);
            Assert.False(unmanagedValue1.SequenceEqual(unmanagedValue2));

            using var unmanagedValue3 = Unmanaged<UnmanagedTest1>.Alloc();
            using var unmanagedValue4 = Unmanaged<UnmanagedTest1>.Alloc();
            unmanagedValue3[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            unmanagedValue4[0] = new UnmanagedTest1 { I1 = 150, I2 = 250 };
            Assert.False(unmanagedValue3.SequenceEqual(unmanagedValue4));

            using var unmanagedValue5 = Unmanaged<UnmanagedTest1>.Alloc(2);
            using var unmanagedValue6 = Unmanaged<UnmanagedTest1>.Alloc(2);
            unmanagedValue5[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            unmanagedValue5[1] = new UnmanagedTest1 { I1 = 101, I2 = 201 };
            unmanagedValue6[0] = new UnmanagedTest1 { I1 = 100, I2 = 200 };
            unmanagedValue6[1] = new UnmanagedTest1 { I1 = 150, I2 = 250 };
            Assert.False(unmanagedValue5.SequenceEqual(unmanagedValue6));

            using var unmanagedValue7 = Unmanaged<short>.Alloc();
            using var unmanagedValue8 = Unmanaged<short>.Alloc();
            unmanagedValue7[0] = 7;
            unmanagedValue8[0] = 8;
            Assert.False(unmanagedValue7.SequenceEqual(unmanagedValue8));
        }
    }
}