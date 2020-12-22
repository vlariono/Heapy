using System;
using System.Runtime.InteropServices;
using Heapy.Core.Heap;

namespace Heapy.Text
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public unsafe struct LPStr
    {
        [FieldOffset(0)] private int _length;
        [FieldOffset(4)] private byte* _ptr;

        public int Length
        {
            get => _length;
            private set => _length = value;
        }

        private byte* Ptr
        {
            get => _ptr;
            set => _ptr = value;
        }

        public byte this[int i]
        {
            get
            {
                if (0 > i && i >= Length)
                {
                    throw new IndexOutOfRangeException(nameof(LPStr));
                }

                return Ptr[i];
            }
            set
            {
                if (0 > i && i >= Length)
                {
                    throw new IndexOutOfRangeException(nameof(LPStr));
                }

                Ptr[i] = value;
            }
        }

        public override string? ToString()
        {
            return Marshal.PtrToStringAnsi((IntPtr)Ptr);
        }

        public static implicit operator IntPtr(LPStr str) => (IntPtr) str.Ptr;
        public static implicit operator byte*(LPStr str) => str.Ptr;

        public static Unmanaged<LPStr> ToLPStr(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(str));
            }

            var heap = HeapManager.Current;
            var structSize = sizeof(LPStr);
            var areaSize = str.Length + structSize + 1;

            var lpStr = heap.Alloc<LPStr>((uint)areaSize);
            lpStr.Value.Length = str.Length;
            lpStr.Value.Ptr = (byte*)lpStr.Ptr + structSize;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                lpStr.Value[i] = Convert.ToByte(c);
            }
            lpStr.Value[str.Length] = 0;

            return lpStr;
        }
    }
}