using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository CreateRepository(IUnitOfWorkAccessor unitOfWorkAccessor);
    }
}
