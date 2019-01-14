using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Examples.Todos.Dto;
using Revo.Examples.Todos.Messages.Queries;
using Revo.Examples.Todos.Reads.Model;

namespace Revo.Examples.Todos.Reads.Handlers
{
    public class TaskListQueryHandler :
        IQueryHandler<GetTodoListsQuery, IQueryable<TodoListDto>>
    {
        private readonly IReadRepository readRepository;

        public TaskListQueryHandler(IReadRepository readRepository)
        {
            this.readRepository = readRepository;
        }

        public Task<IQueryable<TodoListDto>> HandleAsync(GetTodoListsQuery query, CancellationToken cancellationToken)
        {
            IQueryable<TodoListDto> taskLists = readRepository
                .FindAll<TodoListReadModel>()
                .Include(x => x.Todos)
                .ProjectTo<TodoListDto>();
            return Task.FromResult(taskLists);
        }
    }
}
