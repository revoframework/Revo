using System;
using NSubstitute;
using Revo.Core.Security;
using Revo.Extensions.Notifications.Channels.Apns.Model;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Apns
{
    public class ApnsUserDeviceTokenTests
    {
        private readonly IUser user;

        public ApnsUserDeviceTokenTests()
        {
            user = Substitute.For<IUser>();
            user.Id.Returns(Guid.NewGuid());
        }
        
        [Fact]
        public void Constructor_SavesToken()
        {
            var token = new ApnsUserDeviceToken(Guid.NewGuid(), user, "1cc31e97c96b274eb056a969eff7f6d783fcdf6985dc457734837bd45fb1a731", "appid");
            Assert.Equal("1cc31e97c96b274eb056a969eff7f6d783fcdf6985dc457734837bd45fb1a731", token.DeviceToken);
        }

        [Fact]
        public void Constructor_ThrowsIfInvalidToken()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ApnsUserDeviceToken(Guid.NewGuid(), user, "badtoken", "appid");
            });
        }
    }
}
