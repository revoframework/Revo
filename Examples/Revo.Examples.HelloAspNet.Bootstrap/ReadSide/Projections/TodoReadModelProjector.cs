using System;
using Revo.Core.Events;
using Revo.EF6.DataAccess.Entities;
using Revo.EF6.Projections;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.Domain.Events;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Model;

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