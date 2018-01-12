using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public class AsyncEventProcessingSequenceException : AsyncEventProcessingException
    {
        public AsyncEventProcessingSequenceException(long? lastSequenceNumberProcessed)
        {
            LastSequenceNumberProcessed = lastSequenceNumberProcessed;
        }

        public AsyncEventProcessingSequenceException(string message, long? lastSequenceNumberProcessed) : base(message)
        {
            LastSequenceNumberProcessed = lastSequenceNumberProcessed;
        }

        public AsyncEventProcessingSequenceException(string message, Exception innerException, long? lastSequenceNumberProcessed) : base(message, innerException)
        {
            LastSequenceNumberProcessed = lastSequenceNumberProcessed;
        }

        protected AsyncEventProcessingSequenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public long? LastSequenceNumberProcessed { get; }
    }
}
