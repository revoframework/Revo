using System;

namespace Revo.Domain.ReadModel
{
    public interface ITenantReadModel
    {
        Guid? TenantId { get; set; }
    }
}
