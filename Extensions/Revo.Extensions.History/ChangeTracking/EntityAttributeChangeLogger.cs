using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Extensions.History.ChangeTracking.Changes;
using Revo.Extensions.History.ChangeTracking.Model;

namespace Revo.Extensions.History.ChangeTracking
{
    public class EntityAttributeChangeLogger : IEntityAttributeChangeLogger
    {
        private readonly IChangeTracker changeTracker;
        private readonly ICrudRepository crudRepository;

        private readonly Dictionary<TrackedEntityKey, EntityAttributeData> entityAttributes =
            new Dictionary<TrackedEntityKey, EntityAttributeData>();

        public EntityAttributeChangeLogger(IChangeTracker changeTracker,
            ICrudRepository crudRepository)
        {
            this.changeTracker = changeTracker;
            this.crudRepository = crudRepository;
        }

        public EntityAttributeChangeBatch Batch(Guid? aggregateId = null, Guid? aggregateClassId = null,
            Guid? entityId = null, Guid? entityClassId = null, Guid? userId = null)
        {
            return new EntityAttributeChangeBatch(this, aggregateId, aggregateClassId,
                entityId, entityClassId, userId);
        }

        public async Task ChangeAttribute<T>(string attributeName, T newValue, Guid? aggregateId = null,
            Guid? aggregateClassId = null, Guid? entityId = null, Guid? entityClassId = null,
            Guid? userId = null)
        {
            var oldValueTuple = await FindOldValueAndReplace<T>(aggregateId, entityId, attributeName, newValue);

            if (oldValueTuple.Item2 //save only if has an old value
                && !EqualityComparer<T>.Default.Equals(oldValueTuple.Item1, newValue))
            {
                EntityAttributeChangeData<T> changeData = new EntityAttributeChangeData<T>()
                {
                    AttributeName = attributeName,
                    OldValue = oldValueTuple.Item1,
                    NewValue = newValue
                };

                changeTracker.AddChange(changeData, aggregateId, aggregateClassId,
                    entityId, entityClassId, userId);
            }
        }

        /*public async Task ChangeAttributes(Dictionary<string, Tuple<ClrType, object>> newAttributeValues,
            Guid? aggregateId = null, Guid? aggregateClassId = null,
            Guid? entityId = null, Guid? entityClassId = null)
        {
            foreach (var attributeValue in newAttributeValues)
            {
                // TODO better solution without reflection
                var method = this.GetType().GetMethod("ChangeAttribute").MakeGenericMethod(attributeValue.Value.Item1);
                Task task = (Task)method.Invoke(this, new object[] { attributeValue.Key, attributeValue.Value.Item2,
                    aggregateId, aggregateClassId, entityId, entityClassId });
                await task;
            }
        }*/

        private async Task<Tuple<T, bool>> FindOldValueAndReplace<T>(Guid? aggregateId, Guid? entityId,
            string attributeName, T newValue)
        {
            var entityKey = new TrackedEntityKey() {EntityId = entityId, AggregateId = aggregateId};
            EntityAttributeData attributes;
            if (!entityAttributes.TryGetValue(entityKey, out attributes))
            {
                attributes = await crudRepository
                   .FirstOrDefaultAsync<EntityAttributeData>(x => x.AggregateId == aggregateId && x.EntityId == entityId);

                if (attributes == null)
                {
                    attributes = new EntityAttributeData(Guid.NewGuid(), aggregateId, entityId);
                    crudRepository.Add(attributes);
                }

                entityAttributes[entityKey] = attributes;
            }

            T oldValue = default(T);
            bool hasOldValue = attributes.TryGetAttributeValue<T>(attributeName, out oldValue);

            attributes.SetAttributeValue(attributeName, newValue);
            return new Tuple<T, bool>(oldValue, hasOldValue);
        }

        private struct TrackedEntityKey
        {
            public Guid? AggregateId { get; set; }
            public Guid? EntityId { get; set; }
        }
    }
}
