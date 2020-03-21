using AutoMapper;
using Revo.Examples.Todos.Reads.Model;

namespace Revo.Examples.Todos.Dto
{
    public class DtoAutoMapperProfile : Profile
    {
        public DtoAutoMapperProfile()
        {
            CreateMap<TodoListReadModel, TodoListDto>();
            CreateMap<TodoReadModel, TodoDto>();
        }
    }
}
