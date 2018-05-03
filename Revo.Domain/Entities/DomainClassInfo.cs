using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Domain.Entities
{
    public class DomainClassInfo
    {
        public DomainClassInfo(Guid id, string code, Type clrType)
        {
            Id = id;
            Code = code;
            ClrType = clrType;
        }

        public Guid Id { get; }
        public string Code { get; }
        public Type ClrType { get; }
    }
}
