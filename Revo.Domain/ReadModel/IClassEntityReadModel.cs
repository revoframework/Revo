using System;

namespace Revo.Domain.ReadModel
{
    public interface IClassEntityReadModel : IEntityReadModel
    {
        Guid ClassId { get; set; }
    }
}
