using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Security;
using Revo.Infrastructure.Notifications.Channels.Apns.CommandHandlers;
using Revo.Infrastructure.Notifications.Channels.Apns.Commands;
using Revo.Infrastructure.Notifications.Channels.Apns.Model;
using Revo.Testing.Core;
using Revo.Testing.Infrastructure;
using Revo.Testing.Infrastructure.Repositories;
using Revo.Testing.Security;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Notifications.Channels.Apns
{
    public class PushNotificationCommandHandlerTests
    {
        private const string DeviceToken1 = "da8820921cdf472887f458abb67e1db1da8820921cdf472887f458abb67e1db1";

        private readonly FakeUserContext userContext;
        private readonly FakeRepository repository;
        private readonly ApnsNotificationCommandHandler sut;

        public PushNotificationCommandHandlerTests()
        {
            repository = new FakeRepository();
            userContext = new FakeUserContext();
            userContext.SetFakeUser();
            FakeClock.Setup();

            sut = new ApnsNotificationCommandHandler(repository, userContext);
        }

        [Fact]
        public async Task Handle_RegisterApnsDeviceCommand_SavesToken()
        {
            string deviceToken = "da8820921cdf472887f458abb67e1db1da8820921cdf472887f458abb67e1db1";
            await sut.HandleAsync(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = deviceToken
            }, CancellationToken.None);

            await repository.SaveChangesAsync();

            Assert.Equal(1, repository.FindAll<ApnsUserDeviceToken>().Count());
            Assert.Contains(repository.FindAll<ApnsUserDeviceToken>(),
                x => x.UserId == userContext.UserId
                && x.AppId == "My.AppId"
                && x.DeviceToken == deviceToken
                && x.IssuedDateTime == FakeClock.Now);
        }

        [Fact]
        public async Task Handle_RegisterApnsDeviceCommand_SavesToken_SpacesInToken()
        {
            string deviceToken = "da882092 1cdf 4728 87f4 58abb67e1db1 da882092 1cdf 4728 87f4 58abb67e1db1";
            string deviceTokenNormalized = "da8820921cdf472887f458abb67e1db1da8820921cdf472887f458abb67e1db1";
            await sut.HandleAsync(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = deviceToken
            },  CancellationToken.None);

            await repository.SaveChangesAsync();

            Assert.Equal(1, repository.FindAll<ApnsUserDeviceToken>().Count());
            Assert.Contains(repository.FindAll<ApnsUserDeviceToken>(),
                x => x.UserId == userContext.UserId
                     && x.AppId == "My.AppId"
                     && x.DeviceToken == deviceTokenNormalized
                     && x.IssuedDateTime == FakeClock.Now);
        }

        [Fact]
        public async Task Handle_RegisterApnsDeviceCommand_ReplacesIfRegisteredForDifferentUser()
        {
            IUser userTwo = Substitute.For<IUser>();
            userTwo.Id.Returns(Guid.NewGuid());
            var oldToken = new ApnsUserDeviceToken(Guid.NewGuid(), userTwo, DeviceToken1, "My.AppId");

            repository.Aggregates.Add(
                new FakeRepository.EntityEntry(
                    oldToken,
                    FakeRepository.EntityState.Unchanged));
            
            await sut.HandleAsync(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = oldToken.DeviceToken
            }, CancellationToken.None);

            await repository.SaveChangesAsync();

            Assert.Equal(1, repository.FindAll<ApnsUserDeviceToken>().Count());
            Assert.Contains(repository.FindAll<ApnsUserDeviceToken>(),
                x => x.UserId == userContext.UserId
                     && x.AppId == "My.AppId"
                     && x.DeviceToken == oldToken.DeviceToken
                     && x.IssuedDateTime == FakeClock.Now);
        }

        [Fact]
        public async Task Handle_RegisterApnsDeviceCommand_DoesntInsertTwice()
        {
            var oldToken = new ApnsUserDeviceToken(Guid.NewGuid(), userContext.FakeUser, DeviceToken1, "My.AppId");

            repository.Aggregates.Add(
                new FakeRepository.EntityEntry(
                    oldToken,
                    FakeRepository.EntityState.Unchanged));

            await sut.HandleAsync(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = oldToken.DeviceToken
            }, CancellationToken.None);

            Assert.Equal(1, repository.FindAll<ApnsUserDeviceToken>().Count());
            Assert.Contains(repository.FindAll<ApnsUserDeviceToken>(),
                x => x == oldToken
                     && x.UserId == userContext.UserId
                     && x.AppId == "My.AppId"
                     && x.DeviceToken == oldToken.DeviceToken
                     && x.IssuedDateTime == FakeClock.Now);
        }

        [Fact]
        public async Task Handle_DeregisterApnsDeviceCommand_RemovesToken()
        {
            var oldToken = new ApnsUserDeviceToken(Guid.NewGuid(), userContext.FakeUser, DeviceToken1, "My.AppId");

            repository.Aggregates.Add(
                new FakeRepository.EntityEntry(
                    oldToken,
                    FakeRepository.EntityState.Unchanged));

            await sut.HandleAsync(new DeregisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = oldToken.DeviceToken
            }, CancellationToken.None);

            Assert.Empty(repository.FindAll<ApnsUserDeviceToken>());
        }
    }
}
