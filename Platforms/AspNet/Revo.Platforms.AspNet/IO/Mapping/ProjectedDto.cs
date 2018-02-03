using AutoMapper.Attributes;

namespace Revo.Platforms.AspNet.IO.Mapping
{
    public abstract class ProjectedDto<T>
    {
        [MapsFromProperty("")]
        internal T ProjectedSource { get; private set; }
    }
}
