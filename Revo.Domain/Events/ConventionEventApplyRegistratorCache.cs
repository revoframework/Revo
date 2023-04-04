using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Revo.Domain.Entities;

namespace Revo.Domain.Events;

public class ConventionEventApplyRegistratorCache
{
    private static ConcurrentDictionary<Type, EventTypeApplyDelegates> componentTypeDelegates = new();

    public static EventTypeApplyDelegates GetApplyDelegates(Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
        {
            throw new ArgumentException(
                $"Only IComponent-derived types have their convention-based event Apply methods indexed (passed type is '{componentType.FullName}')");
        }

        var delegates = componentTypeDelegates.GetOrAdd(componentType,
            x =>
            {
                var delegates = new EventTypeApplyDelegates();
                CreateTypeDelegates(x, delegates);
                return delegates;
            });
        return delegates;
    }

    private static void CreateTypeDelegates(Type componentType, EventTypeApplyDelegates applyDelegates)
    {
        if (componentType.BaseType != null
            && typeof(IComponent).IsAssignableFrom(componentType.BaseType))
        {
            CreateTypeDelegates(componentType.BaseType, applyDelegates);
        }

        var applyMethods = componentType
            .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic)
            .Where(x => x.Name == "Apply"
                        && x.GetParameters().Length == 1
                        && typeof(DomainAggregateEvent).IsAssignableFrom(x.GetParameters()[0].ParameterType));

        foreach (MethodInfo applyMethod in applyMethods)
        {
            Type eventType = applyMethod.GetParameters()[0].ParameterType;

            // Create an Action<AggregateRoot, IEvent> that does (agg, evt) => ((ConcreteAggregate)agg).Apply((ConcreteEvent)evt)
            var evtParam = Expression.Parameter(typeof(DomainAggregateEvent), "evt");
            var aggParam = Expression.Parameter(typeof(IComponent), "agg");

            var eventDelegate = Expression.Lambda(
                Expression.Call(
                    Expression.Convert(
                        aggParam,
                        componentType),
                    applyMethod,
                    Expression.Convert(
                        evtParam,
                        eventType)),
                aggParam,
                evtParam).Compile();

            applyDelegates[eventType] = (Action<IComponent, DomainAggregateEvent>) eventDelegate;
        }
    }

    public class EventTypeApplyDelegates : Dictionary<Type, Action<IComponent, DomainAggregateEvent>>
    { 
    }
}