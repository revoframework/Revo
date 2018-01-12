using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.EventStore
{
    public interface IEventStoreEventMessage : IEventMessage
    {
        IEventStoreRecord Record { get; }
    }
}
