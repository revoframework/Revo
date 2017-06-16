using AutoMapper;

namespace GTRevo.Platform.Core
{
    public interface IAutoMapperDefinition
    {
        void Configure(IMapperConfigurationExpression config);
    }
}
