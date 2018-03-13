using System;
using AutoMapper.Attributes;
using Newtonsoft.Json;

namespace Revo.Domain.Entities.Basic.Dto
{
    [MapsFrom(typeof(BasicClassEntity))]
    public abstract class ClassEntityDto : EntityDto
    {
        public Guid ClassId { get; set; }
    }
}
