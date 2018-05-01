using System;
using NSubstitute;
using Revo.Core.Lifecycle;
using Xunit;

namespace Revo.Core.Tests.Lifecycle
{
    public class ApplicationConfigurerInitializerTests
    {
        private ApplicationConfigurerInitializer sut;

        [Fact]
        public void InitializesAll()
        {
            var configuersFunc = Substitute.For<Func<IApplicationConfigurer[]>>();
            configuersFunc.Invoke().Returns(new[] {Substitute.For<IApplicationConfigurer>(), Substitute.For<IApplicationConfigurer>() });
            sut = new ApplicationConfigurerInitializer(configuersFunc);

            configuersFunc.DidNotReceive().Invoke();
            sut.ConfigureAll();

            configuersFunc()[0].Received(1).Configure();
            configuersFunc()[1].Received(1).Configure();
        }
    }
}
