using System;
using Heapy.Core.Enum;
using Heapy.Core.Exception;
using Heapy.Core.Interface;
using Heapy.Core.UnmanagedHeap;
using Moq;
using Xunit;

namespace Tests.UnmanagedHeap
{
    public class PrivateHeapTests
    {
        private readonly IPrivateHeapNative _fakeHeapNative;
        private readonly IntPtr _heapPtr;
        private readonly IntPtr _reallocPtr;
        private readonly IntPtr _heapPtrFail;
        private readonly IntPtr _objectPtr;

        private uint _passedOptions;
        private IntPtr _passedPtr;
        private IntPtr _passedHeapPtr;
        private IntPtr _passedLength;

        private struct Unmanaged8
        {
            public int I1 { get; set; }
            public int I2 { get; set; }
        }

        private struct Unmanaged16
        {
            public long L1 { get; set; }
            public long L2 { get; set; }
        }


        public PrivateHeapTests()
        {
            _heapPtr = new IntPtr(204790096);
            _heapPtrFail = new IntPtr(300050000);
            _objectPtr = new IntPtr(204797098);
            _reallocPtr = new IntPtr(659322841);

            var heapNativeMock = new Mock<IPrivateHeapNative>();

            //Returns good handler
            heapNativeMock
                .Setup(native => native.HeapAlloc(
                    It.IsAny<IntPtr>(),
                    It.IsAny<uint>(),
                    It.IsAny<IntPtr>()))
                .Returns((IntPtr ptr, uint opt, IntPtr bytes) => ptr == _heapPtrFail ? IntPtr.Zero : _objectPtr)
                .Callback((IntPtr ptr, uint opt, IntPtr bytes) =>
                    {
                        _passedHeapPtr = ptr;
                        _passedOptions = opt;
                        _passedLength = bytes;
                    });
            heapNativeMock
                .Setup(native => native.HeapReAlloc(
                    It.IsAny<IntPtr>(),
                    It.IsAny<uint>(),
                    It.IsAny<IntPtr>(),
                    It.IsAny<IntPtr>()))
                .Returns((IntPtr ptr, uint opt, IntPtr memory, IntPtr bytes) => ptr == _heapPtrFail ? IntPtr.Zero : _reallocPtr)
                .Callback((IntPtr ptr, uint opt, IntPtr memory, IntPtr bytes) =>
                {
                    _passedHeapPtr = ptr;
                    _passedPtr = memory;
                    _passedOptions = opt;
                    _passedLength = bytes;
                });

            heapNativeMock
                .Setup(native => native.HeapFree(
                        It.IsAny<IntPtr>(),
                        It.IsAny<uint>(),
                        It.IsAny<IntPtr>()))
                .Returns(true);


            _fakeHeapNative = heapNativeMock.Object;
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
        }

        [Fact]
        public void HandleIsZero_StateIsUnavailable()
        {
            var privateHeap = new PrivateHeap(IntPtr.Zero, _fakeHeapNative);
            Assert.Equal(UnmanagedState.Unavailable, privateHeap.State);
        }

        [Fact]
        public void Dispose_HandleIsZeroStateIsUnavailable()
        {
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            Assert.Equal(UnmanagedState.Available, privateHeap.State);

            privateHeap.Dispose();
            Assert.Equal(UnmanagedState.Unavailable, privateHeap.State);
        }

        [Fact]
        public void Alloc_SingleObject()
        {
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            var alloc = privateHeap.Alloc<Unmanaged8>();
            EnsureAllocationIsCorrect(alloc, _objectPtr, _heapPtr,1, privateHeap);
        }

        [Fact]
        public void Alloc_Length()
        {
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            var alloc = privateHeap.Alloc<Unmanaged8>(5);
            EnsureAllocationIsCorrect(alloc, _objectPtr, _heapPtr,5, privateHeap);
        }

        [Fact]
        public void Alloc_WithOptions()
        {
            var options = WindowsHeapOptions.ZeroMemory | WindowsHeapOptions.GenerateExceptions;
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            var alloc = privateHeap.Alloc<Unmanaged8>(10, (uint)options);
            EnsureAllocationIsCorrect(alloc, _objectPtr, _heapPtr,10, privateHeap);
            Assert.Equal((uint)options, _passedOptions);
        }

        [Fact]
        public void ReAlloc_Length()
        {
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            var alloc = privateHeap.Realloc<Unmanaged8>(_objectPtr, 10);
            EnsureAllocationIsCorrect(alloc, _reallocPtr,_heapPtr, 10, privateHeap);
        }

        [Fact]
        public void ReAlloc_WithOptions()
        {
            var options = WindowsHeapOptions.ZeroMemory | WindowsHeapOptions.GenerateExceptions;
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            var alloc = privateHeap.Realloc<Unmanaged8>(_objectPtr, 10, (uint)options);
            EnsureAllocationIsCorrect(alloc, _reallocPtr,_heapPtr, 10, privateHeap);
            Assert.Equal((uint)options, _passedOptions);
        }

        [Fact]
        public void Free_Success()
        {
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            var result = privateHeap.Free(_objectPtr);
            Assert.True(result);
        }

        [Fact]
        public void Alloc_ThrowsOutOfMemory()
        {
            var privateHeap = new PrivateHeap(_heapPtrFail, _fakeHeapNative);
            Assert.Throws<OutOfMemoryException>(() => privateHeap.Alloc<Unmanaged8>());
            Assert.Throws<OutOfMemoryException>(() => privateHeap.Alloc<Unmanaged8>(10));
            Assert.Throws<OutOfMemoryException>(() => privateHeap.Alloc<Unmanaged16>(15,(uint) WindowsHeapOptions.CreateEnableExecute));
        }

        [Fact]
        public void Realloc_ThrowsOutOfMemory()
        {
            var privateHeap = new PrivateHeap(_heapPtrFail, _fakeHeapNative);
            Assert.Throws<OutOfMemoryException>(() => privateHeap.Realloc<Unmanaged8>(_objectPtr,10));
            Assert.Throws<OutOfMemoryException>(() => privateHeap.Realloc<Unmanaged8>(_objectPtr,10,(uint) WindowsHeapOptions.Default));
        }

        [Fact]
        public void Disposed_ThrowsUnavailable()
        {
            var privateHeap = new PrivateHeap(_heapPtr, _fakeHeapNative);
            privateHeap.Dispose();
            Assert.Equal(UnmanagedState.Unavailable,privateHeap.State);
            Assert.Throws<UnmanagedHeapUnavailable>(() => privateHeap.Alloc<Unmanaged8>());
            Assert.Throws<UnmanagedHeapUnavailable>(() => privateHeap.Alloc<Unmanaged8>(10));
            Assert.Throws<UnmanagedHeapUnavailable>(() => privateHeap.Alloc<Unmanaged16>(15,(uint) WindowsHeapOptions.CreateEnableExecute));
            Assert.Throws<UnmanagedHeapUnavailable>(() => privateHeap.Realloc<Unmanaged8>(_objectPtr,10));
            Assert.Throws<UnmanagedHeapUnavailable>(() => privateHeap.Realloc<Unmanaged8>(_objectPtr,10,(uint) WindowsHeapOptions.Default));
            Assert.False(privateHeap.Free(_objectPtr));
        }

        private void EnsureAllocationIsCorrect<T>(Unmanaged<T> alloc, IntPtr handle,IntPtr heapHandle, int length, IUnmanagedHeap heap) where T : unmanaged
        {
            Assert.Equal(handle, alloc.Ptr);
            Assert.Equal(handle, alloc);
            unsafe
            {
                var ptr = (Unmanaged8*)handle;
                Assert.True(ptr == alloc);
            }

            Assert.Equal(UnmanagedState.Available, alloc.State);
            Assert.Equal(length, alloc.Length);

            Span<T> span = alloc;
            Assert.True(span.Length == length);
            ReadOnlySpan<T> roSpan = alloc;
            Assert.True(roSpan.Length == length);

            Assert.True(alloc.Heap == heap);
            Assert.Equal(heapHandle,_passedHeapPtr);
            unsafe
            {
                Assert.Equal((IntPtr)(sizeof(T) * length), _passedLength);
            }
        }
    }
}