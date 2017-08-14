using System.Configuration;

namespace GTRevo.Infrastructure.Notifications.Channels.Push
{
    public class ApnsServiceConfigurationSection : ConfigurationSection
    {
        public const string ConfigurationSectionName = "apnsService";

        [ConfigurationProperty("certificateFilePath", IsRequired = true)]
        public string CertificateFilePath
        {
            get { return (string)base["certificateFilePath"]; }
            set { base["certificateFilePath"] = value; }
        }

        [ConfigurationProperty("certificatePassword", IsRequired = true)]
        public string CertificatePassword
        {
            get { return (string)base["certificatePassword"]; }
            set { base["certificatePassword"] = value; }
        }

        [ConfigurationProperty("isSandboxEnvironment", IsRequired = true)]
        public bool IsSandboxEnvironment
        {
            get { return (bool)base["isSandboxEnvironment"]; }
            set { base["isSandboxEnvironment"] = value; }
        }

    }
}
