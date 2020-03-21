using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Revo.AspNetCore.Web;
using Revo.Examples.Todos.Dto;
using Revo.Examples.Todos.Messages.Commands;
using Revo.Examples.Todos.Messages.Queries;

namespace Revo.Examples.Todos.Api
{
    [Route("api/todo-lists")]
    public class TodoListController : CommandApiController
    {
        [HttpGet("")]
        public Task<IQueryable<TodoListDto>> Get()
        {
            return CommandBus.SendAsync(new GetTodoListsQuery());
        }

        [HttpPost("")]
        public Task Post([FromBody] CreateTodoListDto payload)
        {
            return CommandBus.SendAsync(new CreateTodoListCommand(payload.Id, payload.Name));
        }

        [HttpPut("{id}")]
        public Task Put(Guid id, [FromBody] UpdateTodoListDto payload)
        {
            return CommandBus.SendAsync(new UpdateTodoListCommand(id, payload.Name));
        }

        [HttpPost("{id}")]
        public Task PostTodo(Guid id, [FromBody] AddTodoDto payload)
        {
            return CommandBus.SendAsync(new AddTodoCommand(id, payload.Text));
        }

        [HttpPut("{id}/{todoId}")]
        public Task PutTodo(Guid id, Guid todoId, [FromBody] UpdateTodoDto payload)
        {
            return CommandBus.SendAsync(new UpdateTodoCommand(id, todoId, payload.IsComplete, payload.Text));
        }

        public class CreateTodoListDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class UpdateTodoListDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class AddTodoDto
        {
            public string Text { get; set; }
        }

        public class UpdateTodoDto
        {
            public string Text { get; set; }
            public bool IsComplete { get; set; }
        }
    }
}
