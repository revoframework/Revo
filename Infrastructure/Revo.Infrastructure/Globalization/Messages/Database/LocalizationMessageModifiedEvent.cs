using System;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Globalization.Messages.Database
{
    public class LocalizationMessageModifiedEvent : DomainAggregateEvent
    {
        public LocalizationMessageModifiedEvent(string className, string key, string message,
            string localeCode, Guid? tenantId)
        {
            ClassName = className;
            Key = key;
            Message = message;
            LocaleCode = localeCode;
            TenantId = tenantId;
        }

        public string ClassName { get; private set; }
        public string Key { get; private set; }
        public string Message { get; private set; }
        public string LocaleCode { get; private set; }
        public Guid? TenantId { get; private set; }
    }
}
