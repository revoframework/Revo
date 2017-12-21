using System.Configuration;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns
{
    public class ApnsServiceConfigurationSection : ConfigurationSection
    {
        public const string ConfigurationSectionName = "apnsService";

        [ConfigurationProperty("appConfigurations", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ApnsAppConfigurationCollection),
         AddItemName = "addAppConfiguration",
         ClearItemsName = "clearAppConfiguration",
         RemoveItemName = "removeAppConfiguration")]
        public ApnsAppConfigurationCollection AppConfigurations
        {
            get
            {
                return (ApnsAppConfigurationCollection)base["appConfigurations"];
            }
        }

        public class ApnsAppConfigurationCollection : ConfigurationElementCollection
        {
            public ApnsAppConfigurationElement this[int index]
            {
                get { return (ApnsAppConfigurationElement)BaseGet(index); }
                set
                {
                    if (BaseGet(index) != null)
                    {
                        BaseRemoveAt(index);
                    }
                    BaseAdd(index, value);
                }
            }

            public void Add(ApnsAppConfigurationElement element)
            {
                BaseAdd(element);
            }

            public void Clear()
            {
                BaseClear();
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new ApnsAppConfigurationElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ApnsAppConfigurationElement) element).AppId;
            }

            public void Remove(ApnsAppConfigurationElement element)
            {
                BaseRemove(GetElementKey(element));
            }

            public void RemoveAt(int index)
            {
                BaseRemoveAt(index);
            }

            public void Remove(string name)
            {
                BaseRemove(name);
            }
        }

        public class ApnsAppConfigurationElement : ConfigurationElement
        {
            [ConfigurationProperty("appId", IsRequired = true)]
            public string AppId
            {
                get { return (string)base["appId"]; }
                set { base["appId"] = value; }
            }

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
}
