using System;
using Revo.DataAccess.Entities;

namespace Revo.Domain.Dto
{
    public abstract class EntityDto : IHasId<Guid>
    {
        public Guid Id { get; set; }
    }
}
