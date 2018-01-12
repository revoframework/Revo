using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
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
