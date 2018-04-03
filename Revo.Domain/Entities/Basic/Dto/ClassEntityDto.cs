using System;
using Newtonsoft.Json;

namespace Revo.Domain.Entities.Basic.Dto
{
    public abstract class ClassEntityDto : EntityDto
    {
        public Guid ClassId { get; set; }
    }
}
