using System;

namespace Revo.Domain.ReadModel
{
    public abstract class ClassEntityReadModel : EntityReadModel
    {
        public Guid ClassId { get; set; }
    }
}
