using System;

namespace Revo.Domain.Entities.Attributes
{
    public class DomainClassIdAttribute(string classIdGuid, string code = null) : Attribute
    {
        public Guid ClassId { get; private set; } = Guid.Parse(classIdGuid);
        public string Code { get; private set; } = code;
    }
}
