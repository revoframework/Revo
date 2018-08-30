using System.Configuration;

namespace Revo.AspNet.IO.Resources
{
    public sealed class ResourceManagerConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("pathConfiguration", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(LiveResourcePathCollection),
          AddItemName = "addLiveResourcePath",
          ClearItemsName = "clearLiveResourcePath",
          RemoveItemName = "removeLiveResourcePath")]
        public LiveResourcePathCollection PathConfiguration
        {
            get
            {
                return (LiveResourcePathCollection)base["pathConfiguration"];
            }
        }

        public class LiveResourcePathCollection : ConfigurationElementCollection
        {
            public LiveResourcePath this[int index]
            {
                get { return (LiveResourcePath)BaseGet(index); }
                set
                {
                    if (BaseGet(index) != null)
                    {
                        BaseRemoveAt(index);
                    }
                    BaseAdd(index, value);
                }
            }

            public void Add(LiveResourcePath element)
            {
                BaseAdd(element);
            }

            public void Clear()
            {
                BaseClear();
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new LiveResourcePath();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((LiveResourcePath)element).AssemblyName + ";"
                    + ((LiveResourcePath)element).ProjectPath;
            }

            public void Remove(LiveResourcePath element)
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

        public class LiveResourcePath : ConfigurationElement
        {
            [ConfigurationProperty("assemblyName", IsRequired = true)]
            public string AssemblyName
            {
                get { return (string)base["assemblyName"]; }
                set { base["assemblyName"] = value; }
            }

            [ConfigurationProperty("projectPath", IsRequired = true)]
            public string ProjectPath
            {
                get { return (string)base["projectPath"]; }
                set { base["projectPath"] = value; }
            }
        }
    }
}
