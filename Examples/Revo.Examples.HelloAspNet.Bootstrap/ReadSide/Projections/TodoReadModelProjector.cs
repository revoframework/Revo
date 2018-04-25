using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Revo.Core.Events;
using Revo.DataAccess.EF6.Entities;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.Domain.Events;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Model;
using Revo.Infrastructure.EF6.Projections;

namespace Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Projections
{
    public class TodoReadModelProjector : EF6EntityEventToPocoProjector<Todo, TodoReadModel>
    {
        public TodoReadModelProjector(IEF6CrudRepository repository) : base(repository)
        {
        }

        private void Apply(IEventMessage<TodoAddedEvent> ev, Guid aggregateId, TodoReadModel target)
        {
            target.Title = ev.Event.Title;
        }
    }
}