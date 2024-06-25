using System.Threading.Tasks;
using Revo.Core.Transactions;

namespace Revo.EF6.DataAccess.Entities
{
    public class RepositoryTransaction(EF6CrudRepository repository) : ITransaction
    {
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
