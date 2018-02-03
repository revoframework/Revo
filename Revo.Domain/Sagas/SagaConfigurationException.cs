using System;
using System.Runtime.Serialization;

namespace Revo.Domain.Sagas
{
    public class SagaConfigurationException : Exception
    {
        public SagaConfigurationException()
        {
        }

        public SagaConfigurationException(string message) : base(message)
        {
        }

        public SagaConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SagaConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
