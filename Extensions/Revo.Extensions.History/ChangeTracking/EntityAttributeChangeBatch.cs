using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.History.ChangeTracking
{
    public class EntityAttributeChangeBatch(IEntityAttributeChangeLogger entityAttributeChangeLogger,
            Guid? aggregateId, Guid? aggregateClasId, Guid? entityId, Guid? entityClassId,
            Guid? userId = null)
    {
        private readonly List<Func<Task>> changeActions = new List<Func<Task>>();

        public EntityAttributeChangeBatch Change<T>(string attributeName, T newValue)
        {
            changeActions.Add(() => entityAttributeChangeLogger.ChangeAttribute(
                attributeName, newValue, aggregateId, aggregateClasId,
                entityId, entityClassId, userId));

            return this;
        }

        public async Task FinishAsync()
        {
            foreach (Func<Task> changeAction in changeActions)
            {
                await changeAction.Invoke();
            }
        }
    }
}
