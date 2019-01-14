using System;
using Revo.Domain.Dto;

namespace Revo.Examples.Todos.Dto
{
    public class TodoDto : EntityDto
    {
        public Guid TodoListId { get; set; }
        public bool IsComplete { get; set; }
        public string Text { get; set; }
    }
}
