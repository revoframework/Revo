using System;
using System.Runtime.Serialization;

namespace Revo.Infrastructure.Events.Async
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
