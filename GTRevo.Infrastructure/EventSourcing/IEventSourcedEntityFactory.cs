using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public interface IEventSourcedEntityFactory
    {
        IEntity ConstructEntity(Type entityType, Guid id, params object[] parameters);
    }
}
