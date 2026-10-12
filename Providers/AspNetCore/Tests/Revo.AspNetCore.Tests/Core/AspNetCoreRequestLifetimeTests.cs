using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.AspNetCore.Configuration;
using Revo.AspNetCore.Core;
using Revo.Core.Configuration;
using Revo.Core.Lifecycle;
using Xunit;

namespace Revo.AspNetCore.Tests.Core
{
    public class AspNetCoreRequestLifetimeTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        [Fact]
        public void TryRegisterForDispose_ReturnsFalseWithoutHttpRequest()
        {
            httpContextAccessor.HttpContext.Returns((HttpContext) null);
            var sut = new AspNetCoreRequestLifetime(httpContextAccessor);
            var disposable = Substitute.For<IDisposable>();

            sut.TryRegisterForDispose(disposable).Should().BeFalse();

            disposable.DidNotReceive().Dispose();
        }

        [Fact]
        public async Task TryRegisterForDispose_DisposesWhenRequestCompletes()
        {
            var completionCallbacks = new List<(Func<object, Task> Callback, object State)>();
            var responseFeature = Substitute.For<IHttpResponseFeature>();
            responseFeature.WhenForAnyArgs(x => x.OnCompleted(null, null))
                .Do(ci => completionCallbacks.Add((ci.ArgAt<Func<object, Task>>(0), ci.ArgAt<object>(1))));

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(responseFeature);
            httpContextAccessor.HttpContext.Returns(httpContext);

            var sut = new AspNetCoreRequestLifetime(httpContextAccessor);
            var disposable = Substitute.For<IDisposable>();

            sut.TryRegisterForDispose(disposable).Should().BeTrue();
            disposable.DidNotReceive().Dispose();

            foreach (var (callback, state) in completionCallbacks)
            {
                await callback(state);
            }

            disposable.Received(1).Dispose();
        }

        [Fact]
        public void UseAspNetCore_RegistersRequestLifetime()
        {
            var services = new ServiceCollection();
            services.AddRevo(cfg => cfg.UseAspNetCore());

            using var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IRequestLifetime>().Should().BeOfType<AspNetCoreRequestLifetime>();
        }
    }
}
