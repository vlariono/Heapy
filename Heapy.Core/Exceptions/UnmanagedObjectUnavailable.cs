using System;

namespace Heapy.Core.Exceptions
{
    public class UnmanagedObjectUnavailable : Exception
    {
        public UnmanagedObjectUnavailable() : base()
        {
        }

        public UnmanagedObjectUnavailable(string? message) : base(message)
        {
        }

        public UnmanagedObjectUnavailable(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
