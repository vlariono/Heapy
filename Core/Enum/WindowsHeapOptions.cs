using System;

namespace Heapy.Core.Enum
{
    [Flags]
    public enum WindowsHeapOptions:uint
    {
        /// <summary>
        /// Default options
        /// </summary>
        None = 0,
        /// <summary>
        /// Serialized access is not used when the heap functions access this heap. This option applies to all subsequent heap function calls. 
        /// </summary>
        NoSerialize = 0x00000001,
        /// <summary>
        /// The system will raise an exception to indicate a function failure, such as an out-of-memory condition, instead of returning NULL
        /// </summary>
        GenerateExceptions = 0x00000004,
        /// <summary>
        /// The allocated memory will be initialized to zero. Otherwise, the memory is not initialized to zero.
        /// </summary>
        ZeroMemory = 0x00000008,
        /// <summary>
        /// All memory blocks that are allocated from this heap allow code execution, if the hardware enforces data execution prevention. 
        /// </summary>
        CreateEnableExecute = 0x00040000,
        /// <summary>
        /// There can be no movement when reallocating a memory block. If this value is not specified, the function may move the block to a new location. 
        /// </summary>
        ReallocInPlaceOnly = 0x00000010

    }
}
