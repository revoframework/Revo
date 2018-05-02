using System;

namespace Revo.Domain.Entities.Attributes
{
    public class DomainClassIdAttribute : Attribute
    {
        public DomainClassIdAttribute(string classIdGuid, string code = null)
        {
            ClassId = Guid.Parse(classIdGuid);
            Code = code;
        }

        public Guid ClassId { get; private set; }
        public string Code { get; private set; }
    }
}
