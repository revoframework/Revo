using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Security;
using GTRevo.Infrastructure.Notifications.Channels.Push.Model;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Notifications.Channels.Push
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
