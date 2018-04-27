using System;
using Revo.DataAccess.Entities;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Globalization.Messages.Database
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "LOM")]
    public class LocalizationMessage : TenantBasicAggregateRoot
    {
        public LocalizationMessage(Guid id, string className, string key,
            string message, Locale locale, ITenant tenant) : base(id, tenant)
        {
            ClassName = className;
            Key = key;
            LocaleCode = locale.Code;

            SetMessage(message);
        }

        protected LocalizationMessage()
        {
        }
        
        public string ClassName { get; private set; }
        public string Key { get; private set; }
        public string Message { get; private set; }
        public string LocaleCode { get; private set; }

        public void SetMessage(string message)
        {
            Message = message;
            Publish(new LocalizationMessageModifiedEvent(
                className: ClassName,
                key: Key,
                message: Message,
                localeCode: LocaleCode,
                tenantId: TenantId));
        }

        public void Delete()
        {
            Publish(new LocalizationMessageDeletedEvent()); //automatically checks if already is deleted
            MarkDeleted();
        }
    }
}
