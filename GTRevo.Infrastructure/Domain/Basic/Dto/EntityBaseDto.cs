using System;
using AutoMapper.Attributes;

namespace GTRevo.Infrastructure.Domain.Basic.Dto
{
    [MapsFrom(typeof(BasicEntity))]
    public abstract class EntityBaseDto
    {
        // ReSharper disable once InconsistentNaming
        public Guid ID { get; set; } //uppercased for backward compatibility!
    }
}
