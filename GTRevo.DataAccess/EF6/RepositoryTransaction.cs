using System.Threading.Tasks;
using GTRevo.Platform.Transactions;

namespace GTRevo.DataAccess.EF6
{
    public class RepositoryTransaction : ITransaction
    {
        private readonly Repository repository;

        public RepositoryTransaction(Repository repository)
        {
            this.repository = repository;
        }

        public void Commit()
        {
            repository.SaveChanges();
        }

        public async Task CommitAsync()
        {
            await repository.SaveChangesAsync();
        }

        public void Dispose()
        {
        }
    }
}
