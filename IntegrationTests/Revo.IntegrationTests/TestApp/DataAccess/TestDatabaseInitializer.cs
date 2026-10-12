using System;
using System.Threading.Tasks;
using Revo.Infrastructure.DataAccess;
using Revo.Infrastructure.Repositories;
using Revo.IntegrationTests.TestApp.Domain;

namespace Revo.IntegrationTests.TestApp.DataAccess
{
    /// <summary>
    /// Seeds a list when the application starts.
    /// </summary>
    public class TestDatabaseInitializer : IDatabaseInitializer
    {
        public static readonly Guid SeedListId = Guid.Parse("5EED0000-0000-0000-0000-000000000001");
        public const string SeedListName = "seed";

        public async Task InitializeAsync(IRepository repository)
        {
            if (await repository.FindAsync<TestList>(SeedListId) == null)
            {
                repository.Add(new TestList(SeedListId, SeedListName));
            }
        }
    }
}
