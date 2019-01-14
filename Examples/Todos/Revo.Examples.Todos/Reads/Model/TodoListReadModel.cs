using System.Collections.Generic;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Examples.Todos.Reads.Model
{
    [TablePrefix(NamespacePrefix = "TODOS", ColumnPrefix = "TLI")]
    public class TodoListReadModel : EntityReadModel
    {
        public string Name { get; set; }
        public List<TodoReadModel> Todos { get; set; }
    }
}
