using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
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
            IEntityTypeManager entityTypeManager,
            IUnitOfWork unitOfWork)
        {
            this.commandBus = commandBus;
            this.repository = repository;
            this.entityTypeManager = entityTypeManager;
            MetadataRepository = metadataRepository;

            unitOfWork.AddInnerTransaction(new SagaRepositoryTransaction(this));
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

            Guid sagaClassId = entityTypeManager.GetClassInfoByClrType(saga.GetType()).Id;
            MetadataRepository.AddSaga(saga.Id, sagaClassId);

            sagas[saga.Id] = saga;
        }

        public Task<ISaga> GetAsync(Guid id, Guid classId)
        {
            Type clrType = entityTypeManager.GetClassInfoByClassId(classId).ClrType;
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

        public async Task SendSagaCommandsAsync()
        {
            List<ICommandBase> bufferedCommands = new List<ICommandBase>();

            foreach (var sagaPair in sagas)
            {
                if (sagaPair.Value.IsChanged)
                {
                    bufferedCommands.AddRange(sagaPair.Value.UncommitedCommands);
                }
            }

            foreach (ICommandBase command in bufferedCommands)
            {
                await commandBus.SendAsync(command);
            }
        }

        public async Task UpdateSagaMetadatasAsync()
        {
            foreach (var sagaPair in sagas)
            {
                await MetadataRepository.SetSagaKeysAsync(sagaPair.Value.Id,
                    sagaPair.Value.Keys.SelectMany(x => x.Value.Select(y => new KeyValuePair<string, string>(x.Key, y))));
            }
        }

        private async Task<ISaga> GetAsyncInternal<T>(Guid id) where T : class, ISaga
        {
            return await GetAsync<T>(id);
        }

        protected class SagaRepositoryTransaction : ITransaction
        {
            private readonly SagaRepository repository;

            public SagaRepositoryTransaction(SagaRepository repository)
            {
                this.repository = repository;
            }

            public async Task CommitAsync()
            {
                await repository.UpdateSagaMetadatasAsync();
                await repository.MetadataRepository.SaveChangesAsync();
            }

            public void Dispose()
            {
            }
        }
    }
}
