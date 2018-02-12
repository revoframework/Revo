﻿using System;
using AutoMapper.Attributes;
using Newtonsoft.Json;

namespace Revo.Domain.Entities.Basic.Dto
{
    [MapsFrom(typeof(BasicClassEntity))]
    public abstract class ClassEntityDto : EntityDto
    {
        [JsonProperty(PropertyName = "ClassID")] // TODO only for backward compat, to be removed
        public Guid ClassId { get; set; }
    }
}