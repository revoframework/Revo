using System.Collections.Generic;
using Revo.Domain.Dto;

namespace Revo.Examples.Todos.Dto
{
    public class TodoListDto : EntityDto
    {
        public string Name { get; set; }
        public List<TodoDto> Todos { get; set; }
    }
}
