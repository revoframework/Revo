using AutoMapper;

namespace Revo.Platforms.AspNet.Core
{
    public interface IAutoMapperDefinition
    {
        void Configure(IMapperConfigurationExpression config);
    }
}
