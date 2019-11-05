using System;
using System.Runtime.Serialization;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationException : Exception
    {
        public DatabaseMigrationException()
        {
        }

        protected DatabaseMigrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DatabaseMigrationException(string message) : base(message)
        {
        }

        public DatabaseMigrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}