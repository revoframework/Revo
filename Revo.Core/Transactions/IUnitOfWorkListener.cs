using System.Threading.Tasks;

namespace Revo.Core.Transactions
{
    public interface IUnitOfWorkListener
    {
        Task OnBeforeWorkCommitAsync(IUnitOfWork unitOfWork);
        void OnWorkBegin(IUnitOfWork unitOfWork);
        Task OnWorkSucceededAsync(IUnitOfWork unitOfWork);
    }
}
