using AutoMapper.Attributes;
using Newtonsoft.Json;

namespace GTRevo.Platform.IO.Mapping
{
    public abstract class ProjectedDto<T>
    {
        [MapsFromProperty("")]
        internal T ProjectedSource { get; private set; }
    }
}
