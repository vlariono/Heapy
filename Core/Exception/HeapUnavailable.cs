namespace Heapy.Core.Exception
{
    public sealed class UnmanagedHeapUnavailable : System.Exception
    {
        public UnmanagedHeapUnavailable()
        {
            Message = "Heap is unavailable";
        }

        public UnmanagedHeapUnavailable(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }

    public sealed class UnmanagedObjectUnavailable : System.Exception
    {
        public UnmanagedObjectUnavailable()
        {
            Message = "Object is unavailable";
        }

        public UnmanagedObjectUnavailable(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }
}