using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Commands;
using Revo.Core.Commands.Configuration;
using Revo.Core.Configuration;
using Revo.Core.Transactions;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    /// <summary>
    /// Tests the service scoping of commands with the real command bus configuration.
    /// </summary>
    public class LocalCommandBusScopingTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope outerScope;
        private readonly ICommandBus commandBus;

        public LocalCommandBusScopingTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddRevo(cfg => cfg.ConfigureCommands(commands => commands.AutoDiscoverCommandHandlers = false));
            services.AddScoped<ScopeProbe>();
            services.AddScoped<ICommandHandler<ProbeCommand, ProbeResult>, ProbeCommandHandler>();

            serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });

            outerScope = serviceProvider.CreateScope();
            commandBus = outerScope.ServiceProvider.GetRequiredService<ICommandBus>();
        }

        public void Dispose()
        {
            outerScope.Dispose();
            serviceProvider.Dispose();
        }

        [Fact]
        public async Task SendAsync_EachCommandGetsItsOwnScope()
        {
            var result1 = await commandBus.SendAsync(new ProbeCommand());
            var result2 = await commandBus.SendAsync(new ProbeCommand());

            result1.Probe.Should().NotBeSameAs(result2.Probe);
            result1.Probe.Should().NotBeSameAs(outerScope.ServiceProvider.GetRequiredService<ScopeProbe>());
        }

        [Fact]
        public async Task SendAsync_DisposesCommandScopeAfterCommandFinishes()
        {
            var result = await commandBus.SendAsync(new ProbeCommand());

            result.ProbeDisposedDuringHandling.Should().BeFalse();
            result.Probe.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task SendAsync_HandlerSeesItsCommandContextAndUnitOfWork()
        {
            var command = new ProbeCommand();

            var result = await commandBus.SendAsync(command);

            result.CurrentCommand.Should().BeSameAs(command);
            result.UnitOfWork.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_NestedCommandGetsItsOwnScopeAndUnitOfWork()
        {
            var command = new ProbeCommand() { SendNestedCommand = true };

            var result = await commandBus.SendAsync(command);

            result.Nested.Should().NotBeNull();
            result.Nested.Probe.Should().NotBeSameAs(result.Probe);
            result.Nested.UnitOfWork.Should().NotBeSameAs(result.UnitOfWork);
            result.Nested.CurrentCommand.Should().NotBeSameAs(command);
            result.CurrentCommandAfterNested.Should().BeSameAs(command);
            result.Nested.Probe.IsDisposed.Should().BeTrue();
            result.ProbeDisposedDuringHandling.Should().BeFalse();
        }

        public class ScopeProbe : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class ProbeCommand : ICommand<ProbeResult>
        {
            public bool SendNestedCommand { get; set; }
        }

        public class ProbeResult
        {
            public ScopeProbe Probe { get; set; }
            public bool ProbeDisposedDuringHandling { get; set; }
            public ICommandBase CurrentCommand { get; set; }
            public ICommandBase CurrentCommandAfterNested { get; set; }
            public IUnitOfWork UnitOfWork { get; set; }
            public ProbeResult Nested { get; set; }
        }

        public class ProbeCommandHandler(
            ScopeProbe probe,
            ICommandContext commandContext,
            IServiceProvider serviceProvider,
            ICommandBus commandBus)
            : ICommandHandler<ProbeCommand, ProbeResult>
        {
            public async Task<ProbeResult> HandleAsync(ProbeCommand command, CancellationToken cancellationToken)
            {
                var result = new ProbeResult
                {
                    Probe = probe,
                    CurrentCommand = commandContext.CurrentCommand,
                    UnitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>()
                };

                if (command.SendNestedCommand)
                {
                    result.Nested = await commandBus.SendAsync(new ProbeCommand(), cancellationToken);
                }

                result.CurrentCommandAfterNested = commandContext.CurrentCommand;
                result.ProbeDisposedDuringHandling = probe.IsDisposed;
                return result;
            }
        }
    }
}
