using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Revo.Domain.Events;

namespace Revo.Examples.HelloAspNet.Bootstrap.Domain.Events
{
    public class TodoAddedEvent : DomainAggregateEvent
    {
        public TodoAddedEvent(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}