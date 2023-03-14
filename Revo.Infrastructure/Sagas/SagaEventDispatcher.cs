using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.Sagas
{
    public class SagaEventDispatcher : ISagaEventDispatcher
    {
        private readonly ISagaLocator[] sagaLocators;
        private readonly ISagaRepository sagaRepository;
        private readonly IEntityFactory entityFactory;

        public SagaEventDispatcher(ISagaLocator[] sagaLocators, ISagaRepository sagaRepository,
            IEntityFactory entityFactory)
        {
            this.sagaLocators = sagaLocators;
            this.sagaRepository = sagaRepository;
            this.entityFactory = entityFactory;
        }

        public async Task DispatchEventsToSagas(IEnumerable<IEventMessage<DomainEvent>> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                HashSet<LocatedSaga> locatedSagas = new HashSet<LocatedSaga>();
                foreach (ISagaLocator sagaLocator in sagaLocators)
                {
                    var addedSagas = await sagaLocator.LocateSagasAsync(eventMessage);
                    foreach (var addedSaga in addedSagas)
                    {
                        locatedSagas.Add(addedSaga);
                    }
                }

                foreach (LocatedSaga locatedSaga in locatedSagas)
                {
                    ISaga saga;
                    if (locatedSaga.Id.HasValue)
                    {
                        var getTask = (Task<ISaga>) GetType().GetMethod(nameof(GetSagaAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(new[] {locatedSaga.SagaType}).Invoke(this, new object[] { locatedSaga.Id });
                        saga = await getTask;

                    }
                    else
                    {
                        saga = (ISaga) entityFactory.ConstructEntity(locatedSaga.SagaType, Guid.NewGuid());
                        sagaRepository.Add(saga);
                    }


                    saga.HandleEvent(eventMessage);
                }
            }

            await sagaRepository.SendSagaCommandsAsync();
        }

        private async Task<ISaga> GetSagaAsync<T>(Guid id) where T : class, ISaga
        {
            return await sagaRepository.GetAsync<T>(id);
        }
    }
}
