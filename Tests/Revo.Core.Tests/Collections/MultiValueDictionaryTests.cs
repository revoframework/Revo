using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Revo.Core.Collections;
using Xunit;

namespace Revo.Core.Tests.Collections
{
    public class MultiValueDictionaryTests
    {
        private readonly MultiValueDictionary<string, string> sut = new MultiValueDictionary<string, string>();

        [Fact]
        public void Ctor_NewIsEmpty()
        {
            sut.Count.Should().Be(0);
            sut.Should().BeEmpty();
            sut.Keys.Should().BeEmpty();
            sut.Values.Should().BeEmpty();
        }

        [Fact]
        public void Add_AddsNewCollection()
        {
            sut.Add("a", "b");
            sut.Should().BeEquivalentTo(new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() { "b" }));
        }

        [Fact]
        public void Add_AddsToSameCollection()
        {
            sut.Add("a", "b");
            sut.Add("a", "c");
            sut.Should().BeEquivalentTo(new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() { "b", "c" }));
        }

        [Fact]
        public void Add_TwoCollections()
        {
            sut.Add("a", "c");
            sut.Add("b", "c");
            sut.Should().BeEquivalentTo(
                new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() { "c" }),
                new KeyValuePair<string, IReadOnlyCollection<string>>("b", new List<string>() { "c" }));
        }

        [Fact]
        public void AddRange_KeyWithValues()
        {
            sut.AddRange("a", new List<string>() { "1", "2" });
            
            sut.Should().BeEquivalentTo(
                new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() { "1", "2" }));
        }

        [Fact]
        public void AddRange_Pairs()
        {
            var list = new List<KeyValuePair<string, IReadOnlyCollection<string>>>()
            {
                new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() {"1", "2"})
            };

            sut.AddRange(list);
            
            sut.Should().BeEquivalentTo(list);
        }

        [Fact]
        public void AsIEnumerableNonGeneric()
        {
            sut.Add("a", "c");
            sut.Add("b", "c");

            IEnumerable enumerable = sut;
            enumerable.Should().HaveCount(2);
            enumerable.Should().BeEquivalentTo(
                new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() { "c" }),
                new KeyValuePair<string, IReadOnlyCollection<string>>("b", new List<string>() { "c" }));
        }

        [Fact]
        public void Clear_RemovesAll()
        {
            sut.Add("a", "c");
            sut.Add("b", "c");
            sut.Clear();
            
            sut.Should().BeEmpty();
        }

        [Fact]
        public void ContainsKey()
        {
            sut.Add("a", "c");

            sut.ContainsKey("a").Should().BeTrue();
            sut.ContainsKey("b").Should().BeFalse();
        }

        [Fact]
        public void Count_GeturnsNumOfCollections()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");
            sut.Add("b", "3");

            sut.Count.Should().Be(2);
        }

        [Fact]
        public void Keys_GetsKeyCollection()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");
            sut.Add("b", "3");

            sut.Keys.Should().BeEquivalentTo("a", "b");
        }

        [Fact]
        public void Values_GetsCollectionsOfValues()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");
            sut.Add("b", "3");

            sut.Values.Should().BeEquivalentTo(
                new[] {"1", "2"},
                new[] {"3"});
        }

        [Fact]
        public void Remove_SinglePair()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");
            sut.Add("b", "3");
            sut.Remove("a", "2");
            
            sut.Should().BeEquivalentTo(
                new KeyValuePair<string, IReadOnlyCollection<string>>("a", new List<string>() { "1" }),
                new KeyValuePair<string, IReadOnlyCollection<string>>("b", new List<string>() { "3" }));
        }

        [Fact]
        public void Remove_SinglePair_RemovesCollectionIfLast()
        {
            sut.Add("a", "1");
            sut.Remove("a", "1");
            
            sut.Should().BeEmpty();
        }

        [Fact]
        public void Remove_AllWithKey()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");
            sut.Add("b", "3");
            sut.Remove("a");
            
            sut.Should().BeEquivalentTo(
                new KeyValuePair<string, IReadOnlyCollection<string>>("b", new List<string>() { "3" }));
        }

        [Fact]
        public void Remove_AllWithKey_RemovesCollectionIfLast()
        {
            sut.Add("a", "1");
            sut.Remove("a");
            
            sut.Should().BeEmpty();
        }

        [Fact]
        public void This_ReturnsCollection()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");

            IReadOnlyCollection<string> value = sut["a"];
            value.Count.Should().Be(2);
            value.Should().BeEquivalentTo("1", "2");
        }

        [Fact]
        public void This_ThrowsIfKeyNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() =>
            {
                IReadOnlyCollection<string> value = sut["a"];
            });
        }

        [Fact]
        public void TryGetValue_Exists()
        {
            sut.Add("a", "1");
            sut.Add("a", "2");

            bool result = sut.TryGetValue("a", out IReadOnlyCollection<string> value);

            result.Should().BeTrue();
            value.Count.Should().Be(2);
            value.Should().BeEquivalentTo("1", "2");
        }

        [Fact]
        public void TryGetValue_NotExists()
        {
            sut.Add("a", "1");

            bool result = sut.TryGetValue("b", out IReadOnlyCollection<string> value);
            result.Should().BeFalse();
        }
    }
}
