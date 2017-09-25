using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Core.Domain
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
