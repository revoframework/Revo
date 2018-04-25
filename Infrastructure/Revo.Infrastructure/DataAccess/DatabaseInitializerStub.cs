using System.Threading.Tasks;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess
{
    public abstract class DatabaseInitializerStub : IDatabaseInitializer
    {
        private bool isInitialized = false;

        public async Task InitializeAsync(IRepository repository)
        {
            if (!isInitialized)
            {
                await DoInitializeAsync(repository);
                isInitialized = true;
            }
        }

        protected abstract Task DoInitializeAsync(IRepository repository);
    }
}
