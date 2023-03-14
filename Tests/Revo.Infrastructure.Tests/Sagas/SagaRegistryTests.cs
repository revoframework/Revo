using System;
using FluentAssertions;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Infrastructure.Sagas;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaRegistryTests
    {
        private readonly SagaRegistry sut;

        public SagaRegistryTests()
        {
            sut = new SagaRegistry();
        }

        [Fact]
        public void AddAndLookupRegistrations()
        {
            var s1e1 = SagaEventRegistration.AlwaysStarting(typeof(Saga1), typeof(Event1));
            var s1e2 = SagaEventRegistration.AlwaysStarting(typeof(Saga1), typeof(Event2));
            var s2e1 = SagaEventRegistration.AlwaysStarting(typeof(Saga2), typeof(Event1));
            sut.Add(s1e1);
            sut.Add(s1e2);
            sut.Add(s2e1);

            var registrations = sut.LookupRegistrations(typeof(Event1));

            registrations.Should().BeEquivalentTo(new[]
            {
                s1e1, s2e1
            });
        }

        [Fact]
        public void LookupRegistrations_Empty()
        {
            var registrations = sut.LookupRegistrations(typeof(Event1));
            registrations.Should().BeEmpty();
        }

        public class Saga1 : BasicSaga
        {
            public Saga1(Guid id) : base(id)
            {
            }
        }

        public class Saga2 : BasicSaga
        {
            public Saga2(Guid id) : base(id)
            {
            }
        }

        public class Event1 : DomainEvent
        {
        }

        public class Event2 : DomainEvent
        {
        }
    }
}
