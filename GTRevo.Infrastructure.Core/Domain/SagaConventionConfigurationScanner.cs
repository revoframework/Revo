using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Attributes;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain
{
    public static class SagaConventionConfigurationScanner
    {
        public static SagaConfigurationInfo GetSagaConfiguration(Type sagaType)
        {
            Dictionary<Type, SagaConventionEventInfo> events = new Dictionary<Type, SagaConventionEventInfo>();
            AddEvents(sagaType, events);

            return new SagaConfigurationInfo(events);
        }

        private static void AddEvents(Type sagaType, Dictionary<Type, SagaConventionEventInfo> events)
        {
            if (sagaType.BaseType != null
                && typeof(Saga).IsAssignableFrom(sagaType.BaseType))
            {
                AddEvents(sagaType.BaseType, events);
            }

            var handleMethods = sagaType
                .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic)
                .Where(x => x.Name == "Handle"
                            && x.GetParameters().Length == 1
                            && typeof(IEventMessage<DomainEvent>).IsAssignableFrom(x.GetParameters()[0].ParameterType));

            foreach (MethodInfo handleMethod in handleMethods)
            {
                Type messageType = handleMethod.GetParameters()[0].ParameterType;
                Type eventType = handleMethod.GetParameters()[0].ParameterType.GetGenericArguments()[0];

                // Create an Action<AggregateRoot, IEvent> that does (agg, evt) => ((ConcreteAggregate)agg).Handle((ConcreteEvent)evt)
                var evtParam = Expression.Parameter(typeof(IEventMessage<DomainEvent>), "evt");
                var aggParam = Expression.Parameter(typeof(Saga), "agg");

                var eventDelegate = Expression.Lambda(
                    Expression.Call(
                        Expression.Convert(
                            aggParam,
                            sagaType),
                        handleMethod,
                        Expression.Convert(
                            evtParam,
                            messageType)),
                    aggParam,
                    evtParam).Compile();

                var handleDelegate = (Action<Saga, IEventMessage<DomainEvent>>) eventDelegate;

                var sagaEventAttribute = handleMethod.GetCustomAttribute<SagaEventAttribute>(true);
                if (sagaEventAttribute == null)
                {
                    throw new SagaConfigurationException(
                        $"{sagaType.FullName} saga Handle({eventType.FullName}) is missing a SagaEvent attribute");
                }
                
                if (sagaEventAttribute.IsAlwaysStarting)
                {
                    events[eventType] = new SagaConventionEventInfo(handleDelegate);
                }
                else
                {
                    PropertyInfo eventKeyProperty = eventType.GetProperty(sagaEventAttribute.EventKey);
                    if (eventKeyProperty == null)
                    {
                        throw new SagaConfigurationException($"Invalid {sagaType.FullName} saga configuration: can't find event key property '{sagaEventAttribute.EventKey}' in event {eventType.FullName}");
                    }

                    var eventParameter = Expression.Parameter(typeof(DomainEvent), "ev");

                    var getPropertyExpression = Expression.Property(
                        Expression.Convert(
                            eventParameter,
                            eventType),
                        eventKeyProperty);

                    var toStringWithFormatMethod = eventKeyProperty.PropertyType.GetMethod("ToString", new[] { typeof(IFormatProvider) });
                    var toStringMethod = eventKeyProperty.PropertyType.GetMethod("ToString", new[] { typeof(IFormatProvider) });

                    Func<DomainEvent, string> eventKeyExpression;
                    if (toStringWithFormatMethod != null)
                    {
                        eventKeyExpression = (Func<DomainEvent, string>) Expression.Lambda(
                            Expression.Call(getPropertyExpression, toStringWithFormatMethod,
                                Expression.Constant(CultureInfo.InvariantCulture)),
                            eventParameter).Compile();
                    }
                    else
                    {
                        eventKeyExpression = (Func<DomainEvent, string>)Expression.Lambda(
                            Expression.Call(getPropertyExpression, toStringMethod),
                            eventParameter).Compile();
                    }

                    events[eventType] = new SagaConventionEventInfo(eventKeyExpression,
                        sagaEventAttribute.SagaKey, sagaEventAttribute.IsStartingIfSagaNotFound, handleDelegate);
                }
            }
        }
    }
}
