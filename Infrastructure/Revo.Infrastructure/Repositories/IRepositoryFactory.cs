using Revo.Core.Events;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository CreateRepository(IUnitOfWork unitOfWork);
    }
}
