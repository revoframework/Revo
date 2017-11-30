using System.Configuration;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmServiceConfigurationSection : ConfigurationSection
    {
        public const string ConfigurationSectionName = "fcmService";

        [ConfigurationProperty("senderAuthToken", IsRequired = true)]
        public string SenderAuthToken
        {
            get { return (string)base["senderAuthToken"]; }
            set { base["senderAuthToken"] = value; }
        }
    }
}
