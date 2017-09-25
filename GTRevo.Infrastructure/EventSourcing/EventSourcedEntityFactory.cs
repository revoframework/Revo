using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedEntityFactory : IEventSourcedEntityFactory
    {
        public IEntity ConstructEntity(Type entityType, Guid id, params object[] parameters)
        {
            if (!typeof(IEntity).IsAssignableFrom(entityType))
            {
                throw new ArgumentException(
                    $"Cannot construct event sourced entity of type {entityType.FullName} because it's not a valid entity type");
            }

            //TODO: optimize and cache
            ConstructorInfo constructor = entityType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { typeof(Guid) }, new ParameterModifier[] { });

            if (constructor == null)
            {
                throw new InvalidOperationException($"Event sourced entity of type {entityType.FullName} does not have a constructor with 1 Guid parameters");
            }

            IEntity entity = (IEntity)constructor.Invoke(new object[] { id });
            return entity;
        }
    }
}
