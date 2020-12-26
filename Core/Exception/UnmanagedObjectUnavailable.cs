namespace Heapy.Core.Exception
{
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