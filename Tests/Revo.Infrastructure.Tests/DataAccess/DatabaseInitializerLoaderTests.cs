using System.Collections.Generic;
using FluentAssertions;
using Revo.Infrastructure.DataAccess;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Core;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly CommandContextStack commandContextStack;
        private readonly List<IDatabaseInitializer> initializers;


        public DatabaseInitializerLoaderTests()
        {
            initializers = new List<IDatabaseInitializer>()
            {
                Substitute.For<IDatabaseInitializer>(),
                Substitute.For<IDatabaseInitializer>()
            };

            databaseInitializerDiscovery = Substitute.For<IDatabaseInitializerDiscovery>();
            databaseInitializerDiscovery.DiscoverDatabaseInitializers().Returns(initializers);

            unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(unitOfWork);

            repository = Substitute.For<IRepository>();
            commandContextStack = new CommandContextStack();
            
            sut = new DatabaseInitializerLoader(databaseInitializerDiscovery, new DatabaseInitializerDependencySorter(),
                () => repository, () => unitOfWorkFactory, () => commandContextStack);
        }

        [Fact]
        public void OnApplicationStarted_InitializesAll()
        {
            sut.OnApplicationStarted();

            initializers[0].Received(1).InitializeAsync(repository);
            initializers[1].Received(1).InitializeAsync(repository);
        }

        [Fact]
        public void OnApplicationStarted_InitializesInDifferentTaskContexts()
        {
            TaskContext[] taskContexts = new TaskContext[initializers.Count];

            initializers[0].When(x => x.InitializeAsync(Arg.Any<IRepository>())).Do(ci => taskContexts[0] = TaskContext.Current);
            initializers[1].When(x => x.InitializeAsync(Arg.Any<IRepository>())).Do(ci => taskContexts[1] = TaskContext.Current);

            sut.OnApplicationStarted();

            taskContexts[0].Should().NotBeNull();
            taskContexts[1].Should().NotBeNull();
            taskContexts[0].Should().NotBe(taskContexts[1]);
        }
    }
}
