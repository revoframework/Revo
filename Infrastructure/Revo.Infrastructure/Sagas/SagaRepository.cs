using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.Sagas
{
    public class SagaRepository : EventSourcedRepository<ISaga>, ISagaRepository
    {
        private readonly ICommandBus commandBus;
        private readonly List<ICommand> bufferedCommands = new List<ICommand>();

        public SagaRepository(ICommandBus commandBus,
            IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            ISagaMetadataRepository sagaMetadataRepository)
            : base(eventStore, entityTypeManager, publishEventBuffer, repositoryFilters, eventMessageFactory)
        {
            MetadataRepository = sagaMetadataRepository;
            this.commandBus = commandBus;
        }

        public SagaRepository(ICommandBus commandBus,
            IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            ISagaMetadataRepository sagaMetadataRepository,
            Dictionary<Guid, ISaga> aggregates)
            : base(eventStore, entityTypeManager, publishEventBuffer, repositoryFilters, eventMessageFactory, aggregates)
        {
            MetadataRepository = sagaMetadataRepository;
            this.commandBus = commandBus;
        }

        public ISagaMetadataRepository MetadataRepository { get; }

        public override void SaveChanges()
        {
            throw new NotImplementedException("SagaRepository.SaveChanges is not supported yet");

            base.SaveChanges();
            MetadataRepository.SaveChanges();

            if (bufferedCommands.Count > 0)
            {
                foreach (ICommand command in bufferedCommands)
                {
                    //await commandBus.Send(command);
                }

                bufferedCommands.Clear();
            }
        }

        public override async Task SaveChangesAsync()
        {
            foreach (var aggregate in GetLoadedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    await MetadataRepository.SetSagaMetadataAsync(aggregate.Id,
                        new SagaMetadata(aggregate.Keys.ToImmutableDictionary(x => x.Key,
                            x => x.Value.ToImmutableList())));
                }
            }

            await base.SaveChangesAsync();
            await MetadataRepository.SaveChangesAsync();

            if (bufferedCommands.Count > 0)
            {
                foreach (ICommand command in bufferedCommands)
                {
                    await commandBus.SendAsync(command);
                }

                bufferedCommands.Clear();
            }
        }

        protected override EventSourcedRepository<ISaga> CloneWithFilters(
            IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            Dictionary<Guid, ISaga> aggregates)
        {
            return new SagaRepository(commandBus,
                eventStore,
                entityTypeManager,
                publishEventBuffer,
                repositoryFilters,
                eventMessageFactory,
                MetadataRepository,
                aggregates);
        }

        protected override void CommitAggregates()
        {
            foreach (var aggregate in GetLoadedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    bufferedCommands.AddRange(aggregate.UncommitedCommands);
                }
            }

            base.CommitAggregates();
        }
    }
}
