﻿using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Basic;
using System;

namespace GTRevo.Infrastructure.Security
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "SRT")]
    public class ServiceToken: BasicEntity
    {
        public ServiceToken(Guid id, string serviceName ): base(id)
        {
            ServiceName = serviceName;
            Timestamp = DateTime.Now;
        }

		public ServiceToken()
		{
		}

		public string ServiceName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}