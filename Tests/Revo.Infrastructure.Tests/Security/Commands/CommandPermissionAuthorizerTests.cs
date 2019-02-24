using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Security;
using Revo.Infrastructure.Security.Commands;
using Revo.Testing.Security;
using Xunit;

namespace Revo.Infrastructure.Tests.Security.Commands
{
    public class CommandPermissionAuthorizerTests
    {
        private CommandPermissionAuthorizer sut;
        private ICommandPermissionCache commandPermissionCache;
        private IPermissionAuthorizationMatcher permissionAuthorizationMatcher;
        private FakeUserContext userContext;

        public CommandPermissionAuthorizerTests()
        {
            commandPermissionCache = Substitute.For<ICommandPermissionCache>();
            permissionAuthorizationMatcher = Substitute.For<IPermissionAuthorizationMatcher>();
            userContext = new FakeUserContext();

            sut = new CommandPermissionAuthorizer(commandPermissionCache,
                permissionAuthorizationMatcher, userContext);
        }

        [Fact]
        public async Task AuthorizeCommand_NoAuthentication()
        {
            var command = new TestCommand();

            commandPermissionCache.IsAuthenticationRequired(command).Returns(false);
            commandPermissionCache.GetCommandPermissions(command).Returns(new Permission[0]);
            permissionAuthorizationMatcher.CheckAuthorization((IReadOnlyCollection<Permission>)null, null)
                .ReturnsForAnyArgs(true);

            await sut.PreFilterAsync(command);
            Assert.True(true);
        }

        [Fact]
        public async Task AuthorizeCommand_Authentication()
        {
            var command = new TestCommand();

            commandPermissionCache.IsAuthenticationRequired(command).Returns(true);
            commandPermissionCache.GetCommandPermissions(command).Returns(new Permission[0]);

            await Assert.ThrowsAsync<AuthorizationException>(() =>
                sut.PreFilterAsync(command));
        }

        [Fact]
        public async Task AuthorizeCommand_PermissionsPass()
        {
            var command = new TestCommand();

            commandPermissionCache.IsAuthenticationRequired(command).Returns(false);

            var commandPermissions = new [] { new Permission(new PermissionType(Guid.NewGuid(), "P1"), null, null) };
            commandPermissionCache.GetCommandPermissions(command).Returns(commandPermissions);
            var userPermission = new Permission(new PermissionType(Guid.NewGuid(), "P2"), null, null);
            userContext.FakePermissions = new List<Permission>() { userPermission };

            permissionAuthorizationMatcher.CheckAuthorization(
                    Arg.Is<IReadOnlyCollection<Permission>>(x => x.Count == userContext.FakePermissions.Count
                                                                 && x.All(p => userContext.FakePermissions.Contains(p))),
                    Arg.Is<IReadOnlyCollection<Permission>>(x => x.Count == commandPermissions.Length
                                                                 && x.All(p => commandPermissions.Contains(p))))
                .Returns(true);

            await sut.PreFilterAsync(command);
            Assert.True(true);
        }

        [Fact]
        public async Task AuthorizeCommand_PermissionsFail()
        {
            var command = new TestCommand();

            commandPermissionCache.IsAuthenticationRequired(command).Returns(false);

            var commandPermissions = new[] { new Permission(new PermissionType(Guid.NewGuid(), "P1"), null, null) };
            commandPermissionCache.GetCommandPermissions(command).Returns(commandPermissions);
            var userPermission = new Permission(new PermissionType(Guid.NewGuid(), "P2"), null, null);
            userContext.FakePermissions = new List<Permission>() { userPermission };

            permissionAuthorizationMatcher.CheckAuthorization(
                    Arg.Is<IReadOnlyCollection<Permission>>(x => x.Count == userContext.FakePermissions.Count
                                                                 && x.All(p => userContext.FakePermissions.Contains(p))),
                    Arg.Is<IReadOnlyCollection<Permission>>(x => x.Count == commandPermissions.Length
                                                                 && x.All(p => commandPermissions.Contains(p))))
                .Returns(false);

            await Assert.ThrowsAsync<AuthorizationException>(() =>
                sut.PreFilterAsync(command));
        }

        private class TestCommand : ICommand
        {
        }
    }
}
