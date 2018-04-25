using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Infrastructure.DataAccess;
using NSubstitute;
using Revo.Core.Transactions;
using Revo.Infrastructure.Repositories;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess
{
    public class DatabaseInitializerLoaderTests
    {
        private readonly DatabaseInitializerLoader sut;
        private readonly IDatabaseInitializerDiscovery databaseInitializerDiscovery;
        private readonly IRepository repository;
        private readonly IRepositoryFactory repositoryFactory;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public DatabaseInitializerLoaderTests()
        {
            databaseInitializerDiscovery = Substitute.For<IDatabaseInitializerDiscovery>();

            unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(unitOfWork);

            repository = Substitute.For<IRepository>();
            repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.CreateRepository(unitOfWork).Returns(repository);

            sut = new DatabaseInitializerLoader(databaseInitializerDiscovery, new DatabaseInitializerDependencyComparer(),
                () => repositoryFactory, () => unitOfWorkFactory);
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

            initializer1.Received(1).InitializeAsync(repository);
            initializer2.Received(1).InitializeAsync(repository);
        }
    }
}
