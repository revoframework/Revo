using AutoMapper;

namespace Revo.Extensions.AutoMapper
{
    public interface IAutoMapperInitializer
    {
        MapperConfiguration GetMapperConfiguration();
        IMapper CreateMapper();
    }
}