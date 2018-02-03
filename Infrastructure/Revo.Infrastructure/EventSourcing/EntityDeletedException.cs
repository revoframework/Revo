using System;
using System.Runtime.Serialization;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
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
