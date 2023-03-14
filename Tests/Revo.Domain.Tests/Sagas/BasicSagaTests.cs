using System;
using NSubstitute;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Domain.Sagas.Attributes;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.Domain.Tests.Sagas
{
    public class BasicSagaTests
    {
        [Fact]
        public void HandleEvent_InvokesHandle()
        {
            var sut = Substitute.ForPartsOf<Saga1>(Guid.NewGuid());
            var event1 = new Event1().ToMessageDraft();
            sut.HandleEvent(event1);

            sut.Received(1).Handle(event1);
        }

        [Fact]
        public void HandleEvent_TwoCorrelations()
        {
            var sut = Substitute.ForPartsOf<Saga1>(Guid.NewGuid());
            var event2 = new Event2().ToMessageDraft();
            sut.HandleEvent(event2);

            sut.Received(1).Handle(event2);
        }

        public class Saga1 : BasicSaga
        {
            public Saga1(Guid id) : base(id)
            {
            }

            [SagaEvent(SagaKey = "Foo", EventKey = "Foo")]
            public virtual void Handle(IEventMessage<Event1> ev)
            {
            }

            [SagaEvent(SagaKey = "Foo", EventKey = "Foo")]
            [SagaEvent(SagaKey = "Bar", EventKey = "Bar")]
            public virtual void Handle(IEventMessage<Event2> ev)
            {
            }
        }

        public class Event1 : DomainEvent
        {
            public int Foo { get; set; }
        }

        public class Event2 : DomainEvent
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }
    }
}
