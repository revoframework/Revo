using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Revo.Domain.Events;
using Revo.Domain.Sagas;

namespace Revo.Infrastructure.Sagas
{
    public class SagaTypeConfiguration<T> : ISagaTypeConfiguration where T : ISaga
    {
        private readonly List<SagaEventRegistration> eventRegistrations = new List<SagaEventRegistration>();

        public IEnumerable<SagaEventRegistration> EventRegistrations => eventRegistrations;
        public Type SagaType => typeof(T);

        public SagaTypeConfiguration<T> HandlesByKey<TEvent>(string sagaKey,
            Expression<Func<TEvent, string>> eventKeyExpression, bool isStartingIfSagaNotFound = false) where TEvent : DomainEvent
        {
            var eventKeyExpressionFunc = eventKeyExpression.Compile();

            eventRegistrations.Add(SagaEventRegistration.MatchedByKey(typeof(T), typeof(TEvent),
                domainEvent => eventKeyExpressionFunc((TEvent)domainEvent),
                sagaKey, isStartingIfSagaNotFound));
            return this;
        }

        public SagaTypeConfiguration<T> HandlesAll<TEvent>(bool isStartingIfSagaNotFound = false) where TEvent : DomainEvent
        {
            eventRegistrations.Add(SagaEventRegistration.ToAllExistingInstances(typeof(T), typeof(TEvent),
                isStartingIfSagaNotFound));
            return this;
        }
        
        public SagaTypeConfiguration<T> StartsWith<TEvent>() where TEvent : DomainEvent
        {
            eventRegistrations.Add(SagaEventRegistration.AlwaysStarting(typeof(T), typeof(TEvent)));
            return this;
        }
    }
}
