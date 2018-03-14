using AutoMapper;

namespace Revo.Core.IO
{
    public interface IAutoMapperDefinition
    {
        void Configure(IMapperConfigurationExpression config);
    }
}
