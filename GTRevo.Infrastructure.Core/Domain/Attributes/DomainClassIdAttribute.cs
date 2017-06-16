using System;

namespace GTRevo.Infrastructure.Domain.Attributes
{
    public class DomainClassIdAttribute : Attribute
    {
        public DomainClassIdAttribute(string classIdGuid)
        {
            ClassId = Guid.Parse(classIdGuid);
        }

        public Guid ClassId { get; private set; }
    }
}
