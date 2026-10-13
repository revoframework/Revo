using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public async Task TryRegisterForDispose_ConcurrentRegistrationsAreSerialized()
        {
            // e.g. the synchronous event queues processed in parallel within one request, while HttpContext
            // (and the server's response feature behind it) is not thread-safe
            const int threadCount = 8;
            const int registrationsPerThread = 50;

            var responseFeature = new ConcurrencyDetectingResponseFeature();
            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<IHttpResponseFeature>(responseFeature);
            httpContextAccessor.HttpContext.Returns(httpContext);

            var sut = new AspNetCoreRequestLifetime(httpContextAccessor);
            var disposables = Enumerable.Range(0, threadCount * registrationsPerThread)
                .Select(_ => new CountingDisposable())
                .ToArray();

            using var start = new Barrier(threadCount);
            var threads = Enumerable.Range(0, threadCount)
                .Select(threadIndex => Task.Factory.StartNew(() =>
                {
                    start.SignalAndWait();
                    for (int i = 0; i < registrationsPerThread; i++)
                    {
                        sut.TryRegisterForDispose(disposables[threadIndex * registrationsPerThread + i]);
                    }
                }, TaskCreationOptions.LongRunning))
                .ToArray();
            await Task.WhenAll(threads);

            responseFeature.ConcurrentCallDetected.Should().BeFalse();

            await responseFeature.CompleteAsync();
            disposables.Should().OnlyContain(x => x.DisposeCount == 1);
        }

        [Fact]
        public void UseAspNetCore_RegistersRequestLifetime()
        {
            var services = new ServiceCollection();
            services.AddRevo(cfg => cfg.UseAspNetCore());

            using var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IRequestLifetime>().Should().BeOfType<AspNetCoreRequestLifetime>();
        }

        /// <summary>
        /// Response feature with a deliberately non-thread-safe list of completion callbacks, which detects calls
        /// overlapping with each other.
        /// </summary>
        private sealed class ConcurrencyDetectingResponseFeature : HttpResponseFeature
        {
            private readonly List<(Func<object, Task> Callback, object State)> completionCallbacks = new();
            private int activeCalls;

            public bool ConcurrentCallDetected { get; private set; }

            public override void OnCompleted(Func<object, Task> callback, object state)
            {
                if (Interlocked.Increment(ref activeCalls) > 1)
                {
                    ConcurrentCallDetected = true;
                }

                Thread.SpinWait(1000); // widens the window, so that unsynchronized calls reliably overlap
                completionCallbacks.Add((callback, state));
                Interlocked.Decrement(ref activeCalls);
            }

            public async Task CompleteAsync()
            {
                foreach (var (callback, state) in completionCallbacks)
                {
                    await callback(state);
                }
            }
        }

        private sealed class CountingDisposable : IDisposable
        {
            private int disposeCount;

            public int DisposeCount => disposeCount;

            public void Dispose()
            {
                Interlocked.Increment(ref disposeCount);
            }
        }
    }
}
