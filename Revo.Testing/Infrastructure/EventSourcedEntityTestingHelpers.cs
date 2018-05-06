using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.Testing.Infrastructure
{
    public static class EventSourcedEntityTestingHelpers
    {
        public static void AssertEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly, params DomainAggregateEvent[] expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly,
                expectedEvents
                    .Select<DomainAggregateEvent, Func<(DomainAggregateEvent ev,
                        Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(x =>
                    {
                        return () => (ev: x, eventAssertion: null);
                    }).ToArray(), false);
        }

        public static void AssertEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly,
            params (DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)[]
                expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly,
                expectedEvents
                    .Select<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion),
                        Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(
                        x =>
                        {
                            return () => x;
                        }).ToArray(), false);
        }

        public static void AssertEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly,
            params Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[]
                expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly, expectedEvents, false);
        }

        public static void AssertAllEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly, params DomainAggregateEvent[] expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly,
                expectedEvents
                    .Select<DomainAggregateEvent, Func<(DomainAggregateEvent ev,
                        Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(x =>
                    {
                        return () => (ev: x, eventAssertion: null);
                    }).ToArray(), true);
        }

        public static void AssertAllEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly,
            params (DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)[]
                expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly,
                expectedEvents
                    .Select<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion),
                        Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(
                        x =>
                        {
                            return () => x;
                        }).ToArray(), true);
        }

        public static void AssertAllEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly,
            params Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[]
                expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly, expectedEvents, true);
        }

        public static void AssertConstructorEvents<T>(this T aggregate,
            Action<T> stateAssertion, bool loadEventsOnly, params DomainAggregateEvent[] expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            AssertConstructorEvents(aggregate, stateAssertion, loadEventsOnly,
                expectedEvents
                    .Select<DomainAggregateEvent, Func<(DomainAggregateEvent ev,
                        Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(x =>
                    {
                        return () => (ev: x, eventAssertion: null);
                    }).ToArray());
        }

        public static void AssertConstructorEvents<T>(this T aggregate,
            Action<T> stateAssertion, bool loadEventsOnly,
            params (DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)[]
                expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            AssertConstructorEvents(aggregate, stateAssertion, loadEventsOnly,
                expectedEvents
                    .Select<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion),
                        Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(
                        x =>
                        {
                            return () => x;
                        }).ToArray());
        }

        public static void AssertConstructorEvents<T>(this T aggregate,
            Action<T> stateAssertion, bool loadEventsOnly,
            params Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[]
                expectedEvents)
            where T : EventSourcedAggregateRoot
        {
            T sut;
            if (loadEventsOnly)
            {
                IEntityFactory entityFactory = new EntityFactory();
                sut = (T) entityFactory.ConstructEntity(aggregate.GetType(), aggregate.Id);
            }
            else
            {
                sut = aggregate;
            }

            DoAssertEvents(sut, _ => { }, stateAssertion, loadEventsOnly, expectedEvents, true);
        }
        
        private static void DoAssertEvents<T>(this T aggregate, Action<T> action,
            Action<T> stateAssertion, bool loadEventsOnly,
            Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[] expectedEvents,
            bool assertAllEvents)
            where T : EventSourcedAggregateRoot
        {
            if (!loadEventsOnly)
            {
                List<DomainAggregateEvent> newEvents;

                List<DomainAggregateEvent> originalEvents = aggregate.UncommittedEvents.ToList();
                action(aggregate);

                if (assertAllEvents)
                {
                    newEvents = aggregate.UncommittedEvents.ToList();
                }
                else
                {
                    newEvents = aggregate.UncommittedEvents.Except(originalEvents).ToList();
                }

                newEvents.Should().HaveSameCount(expectedEvents);

                for (int i = 0; i < newEvents.Count; i++)
                {
                    var expectedEvent = expectedEvents[i]();
                    newEvents[i].Should().BeAssignableTo(expectedEvent.ev.GetType());

                    if (expectedEvent.eventAssertion == null)
                    {
                        //quick fix for cases when there are no comparable properties, which causes ShouldBeEquivalentTo to fail
                        var excludedProps = new[]
                        {
                            //nameof(DomainAggregateEvent.Id),
                            nameof(DomainAggregateEvent.AggregateId),
                            //nameof(DomainAggregateEvent.AggregateClassId),
                        };

                        if (expectedEvent.ev.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Count(x => !excludedProps.Contains(x.Name)) > 0)
                        {
                            newEvents[i].Should().BeEquivalentTo(expectedEvent.ev,
                                config => config
                                    //.IncludingAllRuntimeProperties()
                                    .RespectingRuntimeTypes()
                                    //.Excluding(x => x.Id)
                                    .Excluding(x => x.AggregateId)
                                    /*.Excluding(x => x.AggregateClassId)*/);
                        }
                    }
                    else
                    {
                        expectedEvent.eventAssertion(expectedEvent.ev, newEvents[i]);
                    }
                }
            }
            else
            {
                aggregate.ReplayEvents(expectedEvents.Select(x => x().ev));
            }

            stateAssertion(aggregate);
        }
    }
}
