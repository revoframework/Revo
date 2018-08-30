using System;

namespace Revo.Domain.Dto
{
    public abstract class ClassEntityDto : EntityDto
    {
        public Guid ClassId { get; set; }
    }
}
