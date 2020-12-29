using Heapy.Core.Extension;
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

        [Fact]
        public unsafe void EqualsByValue_Equals()
        {
            var value1 = new UnmanagedTest1() { I1 = 100, I2 = 200 };
            var value2 = new UnmanagedTest1() { I1 = 100, I2 = 200 };
            Assert.True(value1.EqualsByValue(value2));

            var value3 = new UnmanagedTest2() { I1 = 1000, L2 = 2000 };
            var value4 = new UnmanagedTest2() { I1 = 1000, L2 = 2000 };
            Assert.True(value3.EqualsByValue(value4));

            var value5 = new UnmanagedTest3() { L1 = 10000, L2 = 20000 };
            var value6 = new UnmanagedTest3() { L1 = 10000, L2 = 20000 };
            Assert.True(value5.EqualsByValue(value6));

            var value7 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            var value8 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            value7.Bytes[1] = 112;
            value8.Bytes[1] = 112;
            Assert.True(value7.EqualsByValue(value8));
        }

        [Fact]
        public unsafe void EqualsByValue_NotEquals()
        {
            var value1 = new UnmanagedTest1() { I1 = 150, I2 = 200 };
            var value2 = new UnmanagedTest1() { I1 = 100, I2 = 200 };
            Assert.False(value1.EqualsByValue(value2));

            var value3 = new UnmanagedTest2() { I1 = 1000, L2 = 2000 };
            var value4 = new UnmanagedTest2() { I1 = 1000, L2 = 2500 };
            Assert.False(value3.EqualsByValue(value4));

            var value5 = new UnmanagedTest3() { L1 = 10000, L2 = 20000 };
            var value6 = new UnmanagedTest3() { L1 = 10050, L2 = 20000 };
            Assert.False(value5.EqualsByValue(value6));

            var value7 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            var value8 = new UnmanagedTest4() { L1 = 10000, L2 = 20000 };
            value7.Bytes[0] = 112;
            value8.Bytes[1] = 112;
            Assert.False(value7.EqualsByValue(value8));
        }
    }
}