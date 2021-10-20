using System;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Security;
using Revo.Infrastructure.Security.Commands;
using Xunit;

namespace Revo.Infrastructure.Tests.Security.Commands
{
    public class CommandPermissionCacheTests
    {
        private const string TestPermissionId = "B54FD7C4-278C-4CD0-A53A-BD1925FD0C83";
        private static readonly PermissionType TestPermissionType = new PermissionType(Guid.Parse(TestPermissionId), "Test permission");

        private CommandPermissionCache sut;
        private IPermissionTypeRegistry permissionTypeRegistry;
        private IPermissionTypeIndexer permissionTypeIndexer;
        private ICommandTypeDiscovery commandTypeDiscovery;

        public CommandPermissionCacheTests()
        {
            permissionTypeRegistry = Substitute.For<IPermissionTypeRegistry>();
            permissionTypeIndexer = Substitute.For<IPermissionTypeIndexer>();
            commandTypeDiscovery = Substitute.For<ICommandTypeDiscovery>();

            commandTypeDiscovery.DiscoverCommandTypes().Returns(new[]
            {
                typeof(NotAuthorizedCommand),
                typeof(AuthenticatedCommand),
                typeof(AuthorizedCommand),
                typeof(GenericAuthorizedCommand<>)
            });

            permissionTypeRegistry.GetPermissionTypeById(Guid.Parse(TestPermissionId))
                .Returns(TestPermissionType);

            sut = new CommandPermissionCache(permissionTypeRegistry, permissionTypeIndexer,
                commandTypeDiscovery);
        }

        [Fact]
        public void GetCommandPermissions_NotAuthorizedCommand()
        {
            sut.GetCommandPermissions(new NotAuthorizedCommand())
                .Should().BeEmpty();
        }

        [Fact]
        public void GetCommandPermissions_AuthenticatedCommand()
        {
            sut.GetCommandPermissions(new AuthenticatedCommand())
                .Should().BeEmpty();
        }

        [Fact]
        public void GetCommandPermissions_AuthorizedCommand()
        {
            sut.GetCommandPermissions(new AuthorizedCommand())
                .Should().BeEquivalentTo(new [] { new Permission(TestPermissionType.Id, null, null) });
        }

        [Fact]
        public void GetCommandPermissions_GenericAuthorizedCommandCommand()
        {
            sut.GetCommandPermissions(new GenericAuthorizedCommand<string>())
                .Should().BeEquivalentTo(new[] { new Permission(TestPermissionType.Id, null, null) });
        }

        [Fact]
        public void IsAuthenticationRequired_NotAuthorizedCommand()
        {
            sut.IsAuthenticationRequired(new NotAuthorizedCommand())
                .Should().BeFalse();
        }

        [Fact]
        public void IsAuthenticationRequired_AuthenticatedCommand()
        {
            sut.IsAuthenticationRequired(new AuthenticatedCommand())
                .Should().BeTrue();
        }

        [Fact]
        public void IsAuthenticationRequired_AuthorizedCommand()
        {
            sut.IsAuthenticationRequired(new AuthorizedCommand())
                .Should().BeFalse();
        }
        
        private class NotAuthorizedCommand : ICommand
        {
        }

        [Authenticated]
        private class AuthenticatedCommand : ICommand
        {
        }
        
        [AuthorizePermissions(TestPermissionId)]
        private class AuthorizedCommand : ICommand
        {
        }

        [AuthorizePermissions(TestPermissionId)]
        private class GenericAuthorizedCommand<T> : ICommand
        {
        }
    }
}