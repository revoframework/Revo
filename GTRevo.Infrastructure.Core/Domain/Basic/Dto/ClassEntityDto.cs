using System;
using AutoMapper.Attributes;

namespace GTRevo.Infrastructure.Domain.Basic.Dto
{
    [MapsFrom(typeof(BasicClassEntity))]
    public abstract class ClassEntityDto : EntityBaseDto
    {
        // ReSharper disable once InconsistentNaming
        public Guid ClassID { get; set; } //uppercased for backward compatibility!
    }
}
