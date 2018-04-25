using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Revo.Examples.HelloAspNet.Bootstrap.Dto;
using Revo.Examples.HelloAspNet.Bootstrap.Messages.Commands;
using Revo.Examples.HelloAspNet.Bootstrap.Messages.Queries;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Model;
using Revo.Platforms.AspNet.Web;

namespace Revo.Examples.HelloAspNet.Bootstrap.Api
{
    [RoutePrefix("api/todos")]
    public class TodosController : CommandApiController
    {
        [Route("")]
        public Task<IQueryable<TodoReadModel>> Get()
        {
            return CommandBus.SendAsync(new GetTodosQuery());
        }

        [Route("")]
        public Task Post(AddTodoDto dto)
        {
            return CommandBus.SendAsync(new AddTodoCommand(dto.Title));
        }
    }
}