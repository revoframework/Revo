namespace Revo.Examples.Todos.Dto
{
    public class TodoListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<TodoDto> Todos { get; set; }
    }
}
