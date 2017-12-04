using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmAppConfiguration
    {
        public FcmAppConfiguration(string appId, GcmConfiguration apnsConfiguration)
        {
            AppId = appId;
            FcmConfiguration = apnsConfiguration;
        }

        public string AppId { get; private set; }
        public GcmConfiguration FcmConfiguration { get; private set; }
    }
}
