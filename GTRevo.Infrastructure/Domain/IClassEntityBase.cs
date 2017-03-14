using System;

namespace GTRevo.Infrastructure.Domain
{
    public interface IClassEntityBase : IEntityBase
    {
        Guid ClassId { get; }
    }
}
