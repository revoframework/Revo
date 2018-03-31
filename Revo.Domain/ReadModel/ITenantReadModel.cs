using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Domain.ReadModel
{
    public interface ITenantReadModel
    {
        Guid? TenantId { get; set; }
    }
}
