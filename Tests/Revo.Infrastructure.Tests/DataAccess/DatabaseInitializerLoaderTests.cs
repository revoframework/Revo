using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Infrastructure.DataAccess;
using NSubstitute;
using Revo.Core.Commands;
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
        private readonly IServiceScopeFactory serviceScopeFactory;
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

            var scopedServiceProvider = Substitute.For<IServiceProvider>();
            scopedServiceProvider.GetService(typeof(IUnitOfWorkFactory)).Returns(unitOfWorkFactory);
            scopedServiceProvider.GetService(typeof(CommandContextStack)).Returns(commandContextStack);
            scopedServiceProvider.GetService(typeof(IRepository)).Returns(repository);

            var scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(scopedServiceProvider);

            serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);

            sut = new DatabaseInitializerLoader(databaseInitializerDiscovery,
                new DatabaseInitializerDependencySorter(), serviceScopeFactory);
        }

        [Fact]
        public async Task StartedAsync_InitializesAll()
        {
            await sut.StartedAsync(CancellationToken.None);

            initializers[0].Received(1).InitializeAsync(repository);
            initializers[1].Received(1).InitializeAsync(repository);
        }
    }
}