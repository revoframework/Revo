using System;

namespace Revo.Domain.ReadModel
{
    public abstract class ClassEntityView : EntityView
    {
        public Guid ClassId { get; set; }
    }
}
