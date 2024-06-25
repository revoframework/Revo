using System;

namespace Revo.Domain.Entities
{
    public class DomainClassInfo(Guid id, string code, Type clrType)
    {
        public Guid Id { get; } = id;
        public string Code { get; } = code;
        public Type ClrType { get; } = clrType;
    }
}
