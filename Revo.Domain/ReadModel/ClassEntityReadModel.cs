using System;

namespace Revo.Domain.ReadModel
{
    public abstract class ClassEntityReadModel : EntityReadModel, IClassEntityReadModel
    {
        public Guid ClassId { get; set; }
    }
}
