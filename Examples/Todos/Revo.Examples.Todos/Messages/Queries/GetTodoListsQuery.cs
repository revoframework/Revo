using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Examples.Todos.Dto;
using Revo.Examples.Todos.Reads.Model;

namespace Revo.Examples.Todos.Messages.Queries
{
    public class GetTodoListsQuery : IQuery<IQueryable<TodoListDto>>
    {
    }
}
