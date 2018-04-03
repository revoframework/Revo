using System;
using Newtonsoft.Json;

namespace Revo.Domain.Entities.Basic.Dto
{
    public abstract class EntityDto
    {
        public Guid Id { get; set; }
    }
}
