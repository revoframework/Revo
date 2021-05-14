using System;
using NSubstitute;
using Revo.Core.Lifecycle;
using Xunit;

namespace Revo.Core.Tests.Lifecycle
{
    public class ApplicationLifecycleNotifierTests
    {
        private ApplicationLifecycleNotifier sut;
        private Func<IApplicationStartingListener[]> startingListenersFunc;
        private Func<IApplicationStartedListener[]> startedListenersFunc;
        private Func<IApplicationStoppingListener[]> stoppingListenersFunc;

        public ApplicationLifecycleNotifierTests()
        {
            startingListenersFunc = Substitute.For<Func<IApplicationStartingListener[]>>();
            startingListenersFunc.Invoke().Returns(new[] { Substitute.For<IApplicationStartingListener>(), Substitute.For<IApplicationStartingListener>() });

            startedListenersFunc = Substitute.For<Func<IApplicationStartedListener[]>>();
            startedListenersFunc.Invoke().Returns(new[] { Substitute.For<IApplicationStartedListener>(), Substitute.For<IApplicationStartedListener>() });

            stoppingListenersFunc = Substitute.For<Func<IApplicationStoppingListener[]>>();
            stoppingListenersFunc.Invoke().Returns(new[] { Substitute.For<IApplicationStoppingListener>(), Substitute.For<IApplicationStoppingListener>() });

            sut = new ApplicationLifecycleNotifier(startingListenersFunc, startedListenersFunc, stoppingListenersFunc);
        }

        [Fact]
        public void NotifyStarting()
        {
            startingListenersFunc.DidNotReceive().Invoke();
            sut.NotifyStarting();

            startingListenersFunc()[0].Received(1).OnApplicationStarting();
            startingListenersFunc()[1].Received(1).OnApplicationStarting();
        }

        [Fact]
        public void NotifyStarted()
        {
            startedListenersFunc.DidNotReceive().Invoke();
            sut.NotifyStarted();

            startedListenersFunc()[0].Received(1).OnApplicationStarted();
            startedListenersFunc()[1].Received(1).OnApplicationStarted();
        }

        [Fact]
        public void NotifyStopping()
        {
            stoppingListenersFunc.DidNotReceive().Invoke();
            sut.NotifyStopping();

            stoppingListenersFunc()[0].Received(1).OnApplicationStopping();
            stoppingListenersFunc()[1].Received(1).OnApplicationStopping();
        }
    }
}
