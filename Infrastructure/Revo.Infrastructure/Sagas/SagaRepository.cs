using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStore;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.Sagas
{
    public class SagaRepository : ISagaRepository
    {
        private readonly ICommandBus commandBus;
        private readonly IRepository repository;
        private readonly IEntityTypeManager entityTypeManager;

        private readonly Dictionary<Guid, ISaga> sagas = new Dictionary<Guid, ISaga>();

        public SagaRepository(ICommandBus commandBus,
            IRepository repository,
            ISagaMetadataRepository metadataRepository,
            IEntityTypeManager entityTypeManager)
        {
            this.commandBus = commandBus;
            this.repository = repository;
            MetadataRepository = metadataRepository;
            this.entityTypeManager = entityTypeManager;
        }

        public ISagaMetadataRepository MetadataRepository { get; }

        public void Add(ISaga saga)
        {
            if (sagas.TryGetValue(saga.Id, out ISaga existing))
            {
                if (saga != existing)
                {
                    throw new ArgumentException($"Saga {saga} with specified ID already exists");
                }

                return;
            }

            repository.Add(saga);

            Guid sagaClassId = entityTypeManager.GetClassIdByClrType(saga.GetType());
            MetadataRepository.AddSaga(saga.Id, sagaClassId);

            sagas[saga.Id] = saga;
        }

        public Task<ISaga> GetAsync(Guid id, Guid classId)
        {
            Type clrType = entityTypeManager.GetClrTypeByClassId(classId);
            return (Task<ISaga>) GetType().GetMethod(nameof(GetAsyncInternal), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(new[] {clrType}).Invoke(this, new object[] { id });
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, ISaga
        {
            if (sagas.TryGetValue(id, out ISaga saga))
            {
                if (saga is T typedSaga)
                {
                    return typedSaga;
                }
                else
                {
                    throw new ArgumentException($"Saga {saga} expected to have be of type {typeof(T)}, but actually is {saga.GetType()}");
                }
            }

            T loadedSaga = await repository.GetAsync<T>(id);
            sagas.Add(id, loadedSaga);
            return loadedSaga;
        }

        public async Task SaveChangesAsync()
        {
            List<ICommandBase> bufferedCommands = new List<ICommandBase>();

            foreach (var sagaPair in sagas)
            {
                await MetadataRepository.SetSagaKeysAsync(sagaPair.Value.Id,
                    sagaPair.Value.Keys.SelectMany(x => x.Value.Select(y => new KeyValuePair<string, string>(x.Key, y))));

                if (sagaPair.Value.IsChanged)
                {
                    bufferedCommands.AddRange(sagaPair.Value.UncommitedCommands);
                }
            }

            foreach (ICommandBase command in bufferedCommands)
            {
                await commandBus.SendAsync(command);
            }

            await repository.SaveChangesAsync();
            await MetadataRepository.SaveChangesAsync();
        }

        private async Task<ISaga> GetAsyncInternal<T>(Guid id) where T : class, ISaga
        {
            return await GetAsync<T>(id);
        }
    }
}
