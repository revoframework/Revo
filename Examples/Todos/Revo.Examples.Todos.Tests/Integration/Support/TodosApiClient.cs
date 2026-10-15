using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Revo.Examples.Todos.Dto;

namespace Revo.Examples.Todos.Tests.Integration.Support
{
    /// <summary>
    /// Client of the Todos app's HTTP API (failing on any unsuccessful response).
    /// </summary>
    public class TodosApiClient(HttpClient client)
    {
        private const string TodoListsUrl = "/api/todo-lists";

        public async Task CreateTodoListAsync(Guid id, string name)
        {
            await (await client.PostAsJsonAsync(TodoListsUrl, new { id, name })).EnsureSuccessAsync();
        }

        public async Task RenameTodoListAsync(Guid id, string name)
        {
            await (await client.PutAsJsonAsync($"{TodoListsUrl}/{id}", new { name })).EnsureSuccessAsync();
        }

        public async Task AddTodoAsync(Guid todoListId, string text)
        {
            await (await client.PostAsJsonAsync($"{TodoListsUrl}/{todoListId}", new { text })).EnsureSuccessAsync();
        }

        public async Task UpdateTodoAsync(Guid todoListId, Guid todoId, string text, bool isComplete)
        {
            await (await client.PutAsJsonAsync($"{TodoListsUrl}/{todoListId}/{todoId}", new { text, isComplete }))
                .EnsureSuccessAsync();
        }

        public async Task<TodoListDto[]> GetTodoListsAsync()
        {
            var response = await client.GetAsync(TodoListsUrl);
            await response.EnsureSuccessAsync();
            return await response.Content.ReadFromJsonAsync<TodoListDto[]>();
        }

        public async Task<TodoListDto> FindTodoListAsync(Guid id)
        {
            return (await GetTodoListsAsync()).SingleOrDefault(x => x.Id == id);
        }
    }
}
