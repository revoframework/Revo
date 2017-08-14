using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Notifications.Channels.Push.CommandHandlers;
using GTRevo.Infrastructure.Notifications.Channels.Push.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Push.Model;
using GTRevo.Infrastructure.Repositories;
using GTRevo.Platform.Security;
using GTRevo.Testing.Infrastructure.Repositories;
using GTRevo.Testing.Platform.Core;
using GTRevo.Testing.Security;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Notifications.Channels.Push
{
    public class PushNotificationCommandHandlerTests
    {
        private const string DeviceToken1 = "da8820921cdf472887f458abb67e1db1da8820921cdf472887f458abb67e1db1";

        private readonly FakeUserContext userContext;
        private readonly FakeRepository repository;
        private readonly PushNotificationCommandHandler sut;

        public PushNotificationCommandHandlerTests()
        {
            repository = new FakeRepository();
            userContext = new FakeUserContext();
            userContext.SetFakeUser();
            FakeClock.Setup();

            sut = new PushNotificationCommandHandler(repository, userContext);
        }

        [Fact]
        public async Task Handle_RegisterApnsDeviceCommand_SavesToken()
        {
            string deviceToken = "da8820921cdf472887f458abb67e1db1da8820921cdf472887f458abb67e1db1";
            await sut.Handle(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = deviceToken
            });

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
            await sut.Handle(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = deviceToken
            });

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
            
            await sut.Handle(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = oldToken.DeviceToken
            });

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

            await sut.Handle(new RegisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = oldToken.DeviceToken
            });

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

            await sut.Handle(new DeregisterApnsDeviceCommand()
            {
                AppId = "My.AppId",
                DeviceToken = oldToken.DeviceToken
            });

            Assert.Empty(repository.FindAll<ApnsUserDeviceToken>());
        }
    }
}
