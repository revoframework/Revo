using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.EventSourcing;
using Revo.Examples.HelloAspNet.Bootstrap.Domain.Events;

namespace Revo.Examples.HelloAspNet.Bootstrap.Domain
{
    [DomainClassId("4EB07015-04FF-46F0-8A6D-4A3D0AE6E8DF")]
    public class Todo : EventSourcedAggregateRoot
    {
        public Todo(Guid id, string title) : base(id)
        {
            Publish(new TodoAddedEvent(title));
        }

        protected Todo(Guid id) : base(id)
        {
        }

        public string Title { get; private set; }

        private void Apply(TodoAddedEvent ev)
        {
            Title = ev.Title;
        }
    }
}