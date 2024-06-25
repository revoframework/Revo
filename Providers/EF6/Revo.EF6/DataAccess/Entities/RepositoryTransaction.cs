using System.Threading.Tasks;
using Revo.Core.Transactions;

namespace Revo.EF6.DataAccess.Entities
{
    public class RepositoryTransaction : ITransaction
    {
        private readonly EF6CrudRepository repository;

        public RepositoryTransaction(EF6CrudRepository repository)
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
