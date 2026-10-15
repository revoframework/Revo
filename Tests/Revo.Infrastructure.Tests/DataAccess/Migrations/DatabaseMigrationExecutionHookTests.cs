using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Infrastructure.DataAccess.Migrations;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations;

public class DatabaseMigrationExecutionHookTests
{
    [Fact]
    public async Task ExecuteAsync_WhenApplyMigrationsUponStartup_SendsCommand()
    {
        var bus = Substitute.For<ILocalCommandBus>();
        var configuration = Substitute.For<IDatabaseMigrationsConfiguration>();
        configuration.ApplyMigrationsUponStartup.Returns(true);
        var environment = Substitute.For<IEnvironment>();

        var sut = new DatabaseMigrationExecutionHook(bus, configuration, environment);
        await sut.ExecuteAsync(CancellationToken.None);

        await bus.Received(1).SendAsync(Arg.Any<ExecuteDatabaseMigrationsCommand>(), CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotDevelopmentAndApplyFalse_DoesNotSend()
    {
        var bus = Substitute.For<ILocalCommandBus>();
        var configuration = Substitute.For<IDatabaseMigrationsConfiguration>();
        configuration.ApplyMigrationsUponStartup.Returns(false);
        var environment = Substitute.For<IEnvironment>();
        environment.IsDevelopment.Returns(false);

        var sut = new DatabaseMigrationExecutionHook(bus, configuration, environment);
        await sut.ExecuteAsync(CancellationToken.None);

        await bus.DidNotReceive()
            .SendAsync(Arg.Any<ExecuteDatabaseMigrationsCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDevelopmentAndApplyNull_SendsCommand()
    {
        var bus = Substitute.For<ILocalCommandBus>();
        var configuration = Substitute.For<IDatabaseMigrationsConfiguration>();
        configuration.ApplyMigrationsUponStartup.Returns((bool?)null);
        var environment = Substitute.For<IEnvironment>();
        environment.IsDevelopment.Returns(true);

        var sut = new DatabaseMigrationExecutionHook(bus, configuration, environment);
        await sut.ExecuteAsync(CancellationToken.None);

        await bus.Received(1).SendAsync(Arg.Any<ExecuteDatabaseMigrationsCommand>(), CancellationToken.None);
    }
}
