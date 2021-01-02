﻿using System;
using System.Runtime.InteropServices;
using Heapy.Core.Enum;
using Heapy.Core.Interface;

namespace Heapy.Core.UnmanagedHeap
{
    /// <summary>
    /// Represents instance of default process heap. This heap is not private and do not support real disposal
    /// </summary>
    public sealed class ProcessHeap : IUnmanagedHeap
    {
        private static readonly ProcessHeap Heap;

        static ProcessHeap()
        {
            Heap = new ProcessHeap();
        }

        private ProcessHeap()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Creates new instance of PrivateHeap
        /// </summary>
        /// <returns><see cref="IUnmanagedHeap"/></returns>
        public static IUnmanagedHeap Create()
        {
            return Heap;
        }

        public bool IsAvailable => true;

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>() where TValue : unmanaged
        {
            return Alloc<TValue>(1);
        }

        /// <inheritdoc />
        public unsafe Unmanaged<TValue> Alloc<TValue>(int length) where TValue : unmanaged
        {
            var cb = new IntPtr(sizeof(TValue) * length);
            var allocHandle = Marshal.AllocHGlobal(cb);
            return new Unmanaged<TValue>(allocHandle, length, this);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Alloc<TValue>(int length, uint options) where TValue : unmanaged
        {
            return Alloc<TValue>(length);
        }

        /// <inheritdoc />
        public unsafe Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length) where TValue : unmanaged
        {
            var cb = new IntPtr(sizeof(TValue) * length);
            var allocHandle = Marshal.ReAllocHGlobal(memory, cb);
            return new Unmanaged<TValue>(allocHandle,length, this);
        }

        /// <inheritdoc />
        public Unmanaged<TValue> Realloc<TValue>(IntPtr memory, int length, uint options) where TValue : unmanaged
        {
            return Realloc<TValue>(memory, length);
        }

        /// <inheritdoc />
        public bool Free(IntPtr memory)
        {
            Marshal.FreeHGlobal(memory);
            return true;
        }
    }
}