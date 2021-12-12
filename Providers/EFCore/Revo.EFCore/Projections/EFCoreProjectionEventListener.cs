using System;
using System.Collections.Generic;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Projections;
using Revo.Infrastructure.Tenancy;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectionEventListener : ProjectionEventListener
    {
        public EFCoreProjectionEventListener(Func<IEFCoreProjectionSubSystem> projectionSubSystemFunc,
            IUnitOfWorkFactory unitOfWorkFactory, Func<CommandContextStack> commandContextStackFunc,
            ITenantProvider tenantProvider, EFCoreProjectionEventSequencer eventSequencer) :
            base(projectionSubSystemFunc, unitOfWorkFactory, commandContextStackFunc, tenantProvider)
        {
            EventSequencer = eventSequencer;
        }
        
        public override IAsyncEventSequencer EventSequencer { get; }
        
        public class EFCoreProjectionEventSequencer : AsyncEventSequencer<DomainAggregateEvent>
        {
            public readonly string QueueNamePrefix = "EFCoreProjectionEventListener:";

            private readonly IEntityTypeManager entityTypeManager;
            private readonly IEFCoreProjectorResolver projectorResolver;

            public EFCoreProjectionEventSequencer(IEntityTypeManager entityTypeManager, IEFCoreProjectorResolver projectorResolver)
            {
                this.entityTypeManager = entityTypeManager;
                this.projectorResolver = projectorResolver;
            }

            protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<DomainAggregateEvent> message)
            {
                Guid? aggregateClassId = message.Metadata.GetAggregateClassId();
                DomainClassInfo classInfo = aggregateClassId != null
                    ? entityTypeManager.TryGetClassInfoByClassId(aggregateClassId.Value)
                    : null;
                
                if (classInfo != null
                    && projectorResolver.HasAnyProjectors(classInfo.ClrType))
                {
                    yield return new EventSequencing()
                    {
                        SequenceName = QueueNamePrefix + message.Event.AggregateId.ToString(),
                        EventSequenceNumber = message.Metadata.GetStreamSequenceNumber()
                    };
                }
            }
            
            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<DomainAggregateEvent> message)
            {
                return true;
            }
        }
    }
}
