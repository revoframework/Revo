using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Types;
using Xunit;

namespace Revo.Core.Tests.Types
{
    public class VersionedTypeRegistryTests
    {
        private VersionedTypeRegistry sut;
        private ITypeIndexer typeIndexer;
        private List<VersionedType> allTypes;

        public VersionedTypeRegistryTests()
        {
            allTypes = new List<VersionedType>()
            {
                new VersionedType(new VersionedTypeId("TestTypeA", 1), typeof(TestTypeA)),
                new VersionedType(new VersionedTypeId("TestTypeB", 1), typeof(TestTypeB)),
                new VersionedType(new VersionedTypeId("TestTypeB", 2), typeof(TestTypeBV2))
            };

            typeIndexer = Substitute.For<ITypeIndexer>();
            typeIndexer.IndexTypes<ITestType>().Returns(allTypes);

            sut = new VersionedTypeRegistry(typeIndexer);
        }
        
        [Fact]
        public void GetAllTypes_ReturnsAll()
        {
            var types = sut.GetAllTypes<ITestType>();
            types.Should().BeEquivalentTo(allTypes);
        }

        [Fact]
        public void GetAllTypes_CachesResults()
        {
            var types = sut.GetAllTypes<ITestType>();
            types = sut.GetAllTypes<ITestType>();
            typeIndexer.Received(1).IndexTypes<ITestType>();
        }

        [Fact]
        public void ClearCache_RefreshesTypes()
        {
            sut.GetAllTypes<ITestType>();

            allTypes.Clear();
            allTypes.AddRange(new[]
            {
                new VersionedType(new VersionedTypeId("TestTypeA", 1), typeof(TestTypeA))
            });
            sut.ClearCache<ITestType>();
            
            var types = sut.GetAllTypes<ITestType>();
            types.Should().HaveCount(1);
        }

        [Fact]
        public void GetTypeInfo_FindsByClrType()
        {
            VersionedType result = sut.GetTypeInfo<ITestType>(typeof(TestTypeA));
            result.Should().Be(allTypes.Single(x => x.ClrType == typeof(TestTypeA)));
        }

        [Fact]
        public void GetTypeInfo_FindsById()
        {
            VersionedType result = sut.GetTypeInfo<ITestType>(new VersionedTypeId("TestTypeB", 2));
            result.Should().Be(allTypes.Single(x => x.ClrType == typeof(TestTypeBV2)));
        }

        [Fact]
        public void GetTypeVersions_GetsAllVersions()
        {
            IReadOnlyCollection<VersionedType> results = sut.GetTypeVersions<ITestType>("TestTypeB");
            results.Should().HaveCount(2);
            results.Should().Contain(allTypes.Single(x => x.ClrType == typeof(TestTypeB)));
            results.Should().Contain(allTypes.Single(x => x.ClrType == typeof(TestTypeBV2)));
        }

        public interface ITestType
        {
        }

        public class TestTypeA : ITestType
        {
        }

        public class TestTypeB : ITestType
        {
        }

        public class TestTypeBV2 : ITestType
        {
        }
    }
}
