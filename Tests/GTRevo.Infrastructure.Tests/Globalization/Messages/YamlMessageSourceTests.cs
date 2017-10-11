using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GTRevo.Infrastructure.Globalization.Messages;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Globalization.Messages
{
    public class YamlMessageSourceTests
    {
        [Fact]
        public void Messages_WithSpaces_HasAll()
        {
            Stream messageStream = new MemoryStream(Encoding.UTF8.GetBytes(
                @"Root: Chicken
Root2:
  Child1: Fish
  Child2: Donut
  Child3: Chips
    SubChild1: Coffee
  Child4: Ham and eggs"
            ));

            YamlMessageSource messageSource = new YamlMessageSource(() => messageStream);

            List<KeyValuePair<string, string>> expected = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Root", "Chicken"),
                new KeyValuePair<string, string>("Root2", ""),
                new KeyValuePair<string, string>("Root2.Child1", "Fish"),
                new KeyValuePair<string, string>("Root2.Child2", "Donut"),
                new KeyValuePair<string, string>("Root2.Child3", "Chips"),
                new KeyValuePair<string, string>("Root2.Child3.SubChild1", "Coffee"),
                new KeyValuePair<string, string>("Root2.Child4", "Ham and eggs")
            };

            Assert.True(messageSource.Messages.All(expected.Contains));
            Assert.Equal(expected.Count, messageSource.Messages.Count);
        }
    }
}
