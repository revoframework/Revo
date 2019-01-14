using System;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Examples.Todos.Reads.Model
{
    [TablePrefix(NamespacePrefix = "TODOS", ColumnPrefix = "TDO")]
    public class TodoReadModel : EntityReadModel
    {
        public Guid TodoListId { get; set; }
        public TodoListReadModel TodoList { get; set; }
        public bool IsComplete { get; set; }
        public string Text { get; set; }
    }
}
