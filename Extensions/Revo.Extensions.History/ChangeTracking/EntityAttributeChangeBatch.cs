using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.History.ChangeTracking
{
    public class EntityAttributeChangeBatch
    {
        private readonly IEntityAttributeChangeLogger entityAttributeChangeLogger;
        private readonly Guid? aggregateId;
        private readonly Guid? aggregateClasId;
        private readonly Guid? entityId;
        private readonly Guid? entityClassId;
        private readonly Guid? userId;

        private readonly List<Func<Task>> changeActions = new List<Func<Task>>();

        public EntityAttributeChangeBatch(IEntityAttributeChangeLogger entityAttributeChangeLogger,
            Guid? aggregateId, Guid? aggregateClasId, Guid? entityId, Guid? entityClassId,
            Guid? userId = null)
        {
            this.entityAttributeChangeLogger = entityAttributeChangeLogger;
            this.aggregateId = aggregateId;
            this.aggregateClasId = aggregateClasId;
            this.entityId = entityId;
            this.entityClassId = entityClassId;
            this.userId = userId;
        }

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
