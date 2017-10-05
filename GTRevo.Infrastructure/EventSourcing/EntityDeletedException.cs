using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EntityDeletedException : EntityNotFoundException
    {
        public EntityDeletedException()
        {
        }

        public EntityDeletedException(string message) : base(message)
        {
        }

        public EntityDeletedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityDeletedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
