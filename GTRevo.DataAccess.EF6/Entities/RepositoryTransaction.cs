using System.Threading.Tasks;
using GTRevo.Transactions;

namespace GTRevo.DataAccess.EF6.Entities
{
    public class RepositoryTransaction : ITransaction
    {
        private readonly CrudRepository repository;

        public RepositoryTransaction(CrudRepository repository)
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
