using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using Xunit;

namespace GTRevo.Testing.Infrastructure
{
    public static class EventSourcedEntityTestingHelpers
    {
        public static void AssertEvents(this EventSourcedAggregateRoot aggregate, Action action,
            Action stateAssertion, bool loadEventsOnly,
            params (DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)[] expectedEvents)
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly,
                expectedEvents.Select<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion),
                    Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(x =>
                {
                    return () => x;
                }).ToArray(), false);
        }

        public static void AssertEvents(this EventSourcedAggregateRoot aggregate, Action action,
            Action stateAssertion, bool loadEventsOnly,
            params Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[]
                expectedEvents)
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly, expectedEvents, false);
        }

        public static void AssertAllEvents(this EventSourcedAggregateRoot aggregate, Action action,
            Action stateAssertion, bool loadEventsOnly,
            params (DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)[] expectedEvents)
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly,
                expectedEvents.Select<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion),
                    Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>>(x =>
                {
                    return () => x;
                }).ToArray(), true);
        }

        public static void AssertAllEvents(this EventSourcedAggregateRoot aggregate, Action action,
            Action stateAssertion, bool loadEventsOnly,
            params Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[]
                expectedEvents)
        {
            DoAssertEvents(aggregate, action, stateAssertion, loadEventsOnly, expectedEvents, true);
        }

        private static void DoAssertEvents(EventSourcedAggregateRoot aggregate, Action action,
            Action stateAssertion, bool loadEventsOnly,
            Func<(DomainAggregateEvent ev, Action<DomainAggregateEvent, DomainAggregateEvent> eventAssertion)>[] expectedEvents,
            bool assertAllEvents)
        {
            if (!loadEventsOnly)
            {
                List<DomainAggregateEvent> newEvents;

                List<DomainAggregateEvent> originalEvents = aggregate.UncommittedEvents.ToList();
                action();

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
                            nameof(DomainAggregateEvent.Id),
                            nameof(DomainAggregateEvent.AggregateId),
                            nameof(DomainAggregateEvent.AggregateClassId),
                        };

                        if (expectedEvent.ev.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Count(x => !excludedProps.Contains(x.Name)) > 0)
                        {
                            newEvents[i].ShouldBeEquivalentTo(expectedEvent.ev,
                                config => config
                                    //.IncludingAllRuntimeProperties()
                                    .RespectingRuntimeTypes()
                                    .Excluding(x => x.Id)
                                    .Excluding(x => x.AggregateId)
                                    .Excluding(x => x.AggregateClassId));
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

            stateAssertion();
        }
    }
}
