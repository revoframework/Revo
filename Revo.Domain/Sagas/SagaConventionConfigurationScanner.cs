using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Revo.Core.Collections;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Domain.Sagas.Attributes;

namespace Revo.Domain.Sagas
{
    public static class SagaConventionConfigurationScanner
    {
        public static SagaConfigurationInfo GetSagaConfiguration(Type sagaType)
        {
            MultiValueDictionary<Type, SagaConventionEventInfo> events = new MultiValueDictionary<Type, SagaConventionEventInfo>();
            AddEvents(sagaType, events);

            return new SagaConfigurationInfo(events);
        }

        private static void AddEvents(Type sagaType, MultiValueDictionary<Type, SagaConventionEventInfo> events)
        {
            if (sagaType.BaseType != null
                && typeof(EventSourcedSaga).IsAssignableFrom(sagaType.BaseType))
            {
                AddEvents(sagaType.BaseType, events);
            }

            var handleMethods = sagaType
                .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.Name == "Handle"
                            && x.GetParameters().Length == 1
                            && typeof(IEventMessage<DomainEvent>).IsAssignableFrom(x.GetParameters()[0].ParameterType));

            foreach (MethodInfo handleMethod in handleMethods)
            {
                Type messageType = handleMethod.GetParameters()[0].ParameterType;
                Type eventType = handleMethod.GetParameters()[0].ParameterType.GetGenericArguments()[0];

                // Create an Action<AggregateRoot, IEvent> that does (agg, evt) => ((ConcreteAggregate)agg).Handle((ConcreteEvent)evt)
                var evtParam = Expression.Parameter(typeof(IEventMessage<DomainEvent>), "evt");
                var aggParam = Expression.Parameter(typeof(ISaga), "agg");

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

                var handleDelegate = (Action<ISaga, IEventMessage<DomainEvent>>) eventDelegate;

                var sagaEventAttributes = handleMethod.GetCustomAttributes<SagaEventAttribute>(true).ToList();
                if (sagaEventAttributes.Count == 0)
                {
                    throw new SagaConfigurationException(
                        $"{sagaType.FullName} saga Handle({eventType.FullName}) is missing a SagaEvent attribute");
                }

                foreach (var sagaEventAttribute in sagaEventAttributes)
                {
                    ValidateAttribute(sagaEventAttribute, handleMethod);

                    if (sagaEventAttribute.IsAlwaysStarting)
                    {
                        events.Add(eventType, new SagaConventionEventInfo(handleDelegate));
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
                        var toStringMethod = eventKeyProperty.PropertyType.GetMethod("ToString", new Type[] { });

                        Func<DomainEvent, string> eventKeyExpression;
                        if (toStringWithFormatMethod != null)
                        {
                            eventKeyExpression = (Func<DomainEvent, string>)Expression.Lambda(
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

                        events.Add(eventType, new SagaConventionEventInfo(eventKeyExpression,
                            sagaEventAttribute.SagaKey, sagaEventAttribute.IsStartingIfSagaNotFound, handleDelegate));
                    }
                }
            }
        }

        private static void ValidateAttribute(SagaEventAttribute sagaEventAttribute, MethodInfo handleMethod)
        {
            if ((sagaEventAttribute.EventKey == null) != (sagaEventAttribute.SagaKey == null))
            {
                throw new InvalidOperationException($"Invalid SagaEvent attribute for {handleMethod}: if EventKey is set, then SagaKey must also be set (and vice-versa)");
            }
        }
    }
}
