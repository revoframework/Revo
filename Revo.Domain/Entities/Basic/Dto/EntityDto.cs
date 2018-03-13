using System;
using AutoMapper.Attributes;
using Newtonsoft.Json;

namespace Revo.Domain.Entities.Basic.Dto
{
    [MapsFrom(typeof(BasicEntity))]
    public abstract class EntityDto
    {
        public Guid Id { get; set; }
    }
}
