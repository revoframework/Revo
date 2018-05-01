using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Lifecycle;
using Xunit;

namespace Revo.Core.Tests.Lifecycle
{
    public class ApplicationStartListenerInitializerTests
    {
        private ApplicationStartListenerInitializer sut;
        private Func<IApplicationStartListener[]> startListenersFunc;
        private Func<IApplicationStopListener[]> stopListenersFunc;

        public ApplicationStartListenerInitializerTests()
        {
            startListenersFunc = Substitute.For<Func<IApplicationStartListener[]>>();
            startListenersFunc.Invoke().Returns(new[] { Substitute.For<IApplicationStartListener>(), Substitute.For<IApplicationStartListener>() });

            stopListenersFunc = Substitute.For<Func<IApplicationStopListener[]>>();
            stopListenersFunc.Invoke().Returns(new[] { Substitute.For<IApplicationStopListener>(), Substitute.For<IApplicationStopListener>() });

            sut = new ApplicationStartListenerInitializer(startListenersFunc, stopListenersFunc);
        }

        [Fact]
        public void InitializesStarted()
        {
            startListenersFunc.DidNotReceive().Invoke();
            sut.InitializeStarted();

            startListenersFunc()[0].Received(1).OnApplicationStarted();
            startListenersFunc()[1].Received(1).OnApplicationStarted();
        }

        [Fact]
        public void DeinitializesStopping()
        {
            stopListenersFunc.DidNotReceive().Invoke();
            sut.DeinitializeStopping();

            stopListenersFunc()[0].Received(1).OnApplicationStopping();
            stopListenersFunc()[1].Received(1).OnApplicationStopping();
        }
    }
}
