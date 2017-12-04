using System.Configuration;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmServiceConfigurationSection : ConfigurationSection
    {
        public const string ConfigurationSectionName = "fcmService";

        [ConfigurationProperty("appConfigurations", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(FcmAppConfigurationCollection),
         AddItemName = "addAppConfiguration",
         ClearItemsName = "clearLiveResourcePath",
         RemoveItemName = "removeLiveResourcePath")]
        public FcmAppConfigurationCollection AppConfigurations
        {
            get
            {
                return (FcmAppConfigurationCollection)base["AppConfigurations"];
            }
        }

        public class FcmAppConfigurationCollection : ConfigurationElementCollection
        {
            public FcmAppConfigurationElement this[int index]
            {
                get { return (FcmAppConfigurationElement)BaseGet(index); }
                set
                {
                    if (BaseGet(index) != null)
                    {
                        BaseRemoveAt(index);
                    }
                    BaseAdd(index, value);
                }
            }

            public void Add(FcmAppConfigurationElement element)
            {
                BaseAdd(element);
            }

            public void Clear()
            {
                BaseClear();
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new FcmAppConfigurationElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((FcmAppConfigurationElement)element).AppId;
            }

            public void Remove(FcmAppConfigurationElement element)
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

        public class FcmAppConfigurationElement : ConfigurationElement
        {
            [ConfigurationProperty("appId", IsRequired = true)]
            public string AppId
            {
                get { return (string)base["appId"]; }
                set { base["appId"] = value; }
            }

            [ConfigurationProperty("senderAuthToken", IsRequired = true)]
            public string SenderAuthToken
            {
                get { return (string)base["senderAuthToken"]; }
                set { base["senderAuthToken"] = value; }
            }
        }
    }
}
