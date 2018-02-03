using System;
using System.Runtime.Serialization;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventProcessingException : Exception
    {
        public AsyncEventProcessingException()
        {
        }

        public AsyncEventProcessingException(string message) : base(message)
        {
        }

        public AsyncEventProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AsyncEventProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
