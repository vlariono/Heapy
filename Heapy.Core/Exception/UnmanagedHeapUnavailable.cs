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
}