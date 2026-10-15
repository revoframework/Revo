using System;

namespace Revo.Examples.Todos.Dto
{
    public class TodoDto
    {
        public Guid Id { get; set; }
        public Guid TodoListId { get; set; }
        public bool IsComplete { get; set; }
        public string Text { get; set; }
    }
}
