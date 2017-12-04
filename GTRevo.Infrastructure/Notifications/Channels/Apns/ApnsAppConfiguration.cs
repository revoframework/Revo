using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns
{
    public class ApnsAppConfiguration
    {
        public ApnsAppConfiguration(string appId, ApnsConfiguration apnsConfiguration)
        {
            AppId = appId;
            ApnsConfiguration = apnsConfiguration;
        }

        public string AppId { get; private set; }
        public ApnsConfiguration ApnsConfiguration { get; private set; }
    }
}
