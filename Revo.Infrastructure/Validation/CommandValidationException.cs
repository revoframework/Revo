using System;
using System.Runtime.Serialization;

namespace Revo.Infrastructure.Validation
{
    public class CommandValidationException : Exception
    {
        public CommandValidationException()
        {
        }

        public CommandValidationException(string message) : base(message)
        {
        }

        public CommandValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
