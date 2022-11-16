using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Examples.BlazorWasmTodos.Reads.Model
{
    [TablePrefix(NamespacePrefix = "TODOS", ColumnPrefix = "TLI")]
    public class TodoListReadModel : EntityReadModel
    {
        public string Name { get; set; }
        public List<TodoReadModel> Todos { get; set; }
    }
}
