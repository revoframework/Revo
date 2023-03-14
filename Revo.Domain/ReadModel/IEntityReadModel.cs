using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.ReadModel
{
    public interface IEntityReadModel : IHasId<Guid>
    {
        new Guid Id { get; set; }
    }
}
