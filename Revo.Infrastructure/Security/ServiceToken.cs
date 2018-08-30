using System;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities.Basic;

namespace Revo.Infrastructure.Security
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "SRT")]
    public class ServiceToken: BasicEntity
    {
        public ServiceToken(Guid id, string serviceName ): base(id)
        {
            ServiceName = serviceName;
            Timestamp = DateTimeOffset.Now;
        }

		public ServiceToken()
		{
		}

		public string ServiceName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
