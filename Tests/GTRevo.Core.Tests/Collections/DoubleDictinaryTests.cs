using GTRevo.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GTRevo.Core.Tests.Collections
{
    public class DoubleDictinaryTests
    {
        private DoubleDictionary<string, string, string> instance;

        public DoubleDictinaryTests()
        {
            instance = new DoubleDictionary<string, string, string>();
        }

        [Fact]
        public void TestInitializer()
        {
            instance = new DoubleDictionary<string, string, string>()
            {
                {"fkey1","skey1","value1"},
                {"fkey2","skey2","value2"},
            };
            Assert.Equal(true, instance.ContainsKeys("fkey1"));
            Assert.Equal(true, instance.ContainsKey("fkey1","skey1"));
            Assert.Equal(false, instance.ContainsKeys("fkey3"));
            Assert.Equal(false, instance.ContainsKey("fkey1","skey2"));
        }

        [Fact]
        public void TestIndexer()
        {
            instance["FC", "Bayern"] = "Muenchen";
            Assert.Equal(true, instance.ContainsKeys("FC"));
            Assert.Equal(true, instance.ContainsKey("FC", "Bayern"));
            Assert.Equal("Muenchen", instance["FC", "Bayern"]);            
        }

        [Fact]
        public void TestAdd()
        {
            instance.Add("FC", "Bayern", "Muenchen");
            instance.Add("FC", "Barcelona", "Divers");
            Assert.Equal("Muenchen", instance["FC", "Bayern"]);
            Assert.Equal("Divers", instance["FC", "Barcelona"]);
        }

        [Fact]
        public void TestAddDuplicate()
        {
            instance.Add("FC", "Bayern", "Muenchen");
            Assert.Throws<InvalidOperationException>(
                () => instance.Add("FC", "Bayern", "Munich")
                );            
        }

        [Fact]
        public void TestAddOrSet()
        {
            instance.AddOrSet("FC", "Bayern", "Muenchen");
            Assert.Equal("Muenchen", instance["FC", "Bayern"]);
        }

        [Fact]
        public void TestAddOrSetDuplicate()
        {
            instance.AddOrSet("FC", "Bayern", "Muenchen");
            instance.AddOrSet("FC", "Bayern", "Munich");
            Assert.Equal("Munich", instance["FC", "Bayern"]);
        }

        [Fact]
        public void TestDeleteKey()
        {
            instance.AddOrSet("FC", "Bayern", "Muenchen");
            instance.AddOrSet("SV", "Sportfreunde", "Haengen");
            instance.DeleteKey("SV", "Sportfreunde");
            Assert.Equal(false, instance.ContainsKey("SV", "Sportfreunde"));
            Assert.Equal(true, instance.ContainsKey("FC", "Bayern"));
            Assert.Equal(true, instance.ContainsKeys("SV"));
        }

        [Fact]
        public void TestDeleteKeys()
        {
            instance.AddOrSet("FC", "Bayern", "Muenchen");
            instance.AddOrSet("SV", "Sportfreunde", "Haengen");
            instance.AddOrSet("FC", "Barcelona", "Divers");
            instance.DeleteKeys("FC");
            Assert.Equal(true, instance.ContainsKey("SV", "Sportfreunde"));
            Assert.Equal(false, instance.ContainsKey("FC", "Bayern"));
            Assert.Equal(false, instance.ContainsKeys("FC"));
        }

    }
}
