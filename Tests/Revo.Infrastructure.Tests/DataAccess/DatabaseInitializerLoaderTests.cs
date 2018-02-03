using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Infrastructure.DataAccess;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess
{
    public class DatabaseInitializerLoaderTests
    {
        private readonly DatabaseInitializerLoader sut;
        private readonly IDatabaseInitializerDiscovery databaseInitializerDiscovery;

        public DatabaseInitializerLoaderTests()
        {
            databaseInitializerDiscovery = Substitute.For<IDatabaseInitializerDiscovery>();
            sut = new DatabaseInitializerLoader(databaseInitializerDiscovery, new DatabaseInitializerDependencyComparer());
        }

        [Fact]
        public void OnApplicationStarted_LoadsInitializers()
        {
            IDatabaseInitializer initializer1 = Substitute.For<IDatabaseInitializer>();
            IDatabaseInitializer initializer2 = Substitute.For<IDatabaseInitializer>();

            databaseInitializerDiscovery.DiscoverDatabaseInitializers().Returns(
                new List<IDatabaseInitializer>()
                {
                    initializer1,
                    initializer2
                });

            sut.OnApplicationStarted();

            initializer1.Received(1).Initialize();
            initializer2.Received(1).Initialize();
        }
    }
}
