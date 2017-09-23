using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using LinqKit;

namespace GTRevo.Infrastructure.Sagas
{
    public class SagaTypeConfiguration<T> : ISagaTypeConfiguration where T : ISaga
    {
        private readonly List<SagaEventRegistration> eventRegistrations = new List<SagaEventRegistration>();

        public IEnumerable<SagaEventRegistration> EventRegistrations => eventRegistrations;
        public Type SagaType => typeof(T);

        public SagaTypeConfiguration<T> Handles<TEvent>(string sagaKey,
            Expression<Func<TEvent, string>> eventKeyExpression, bool isStartingIfSagaNotFound = false) where TEvent : DomainEvent
        {
            var eventKeyExpressionFunc = eventKeyExpression.Compile();

            eventRegistrations.Add(new SagaEventRegistration(typeof(T), typeof(TEvent),
                domainEvent => eventKeyExpressionFunc((TEvent)domainEvent),
                sagaKey, isStartingIfSagaNotFound));
            return this;
        }
        
        public SagaTypeConfiguration<T> StartsWith<TEvent>() where TEvent : DomainEvent
        {
            eventRegistrations.Add(new SagaEventRegistration(typeof(T), typeof(TEvent)));
            return this;
        }
    }
}
