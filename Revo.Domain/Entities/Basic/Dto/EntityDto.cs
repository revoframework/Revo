using System;
using AutoMapper.Attributes;
using Newtonsoft.Json;

namespace Revo.Domain.Entities.Basic.Dto
{
    [MapsFrom(typeof(BasicEntity))]
    public abstract class EntityDto
    {
        [JsonProperty(PropertyName = "ID")] // TODO only for backward compat, to be removed
        public Guid Id { get; set; }
    }
}
