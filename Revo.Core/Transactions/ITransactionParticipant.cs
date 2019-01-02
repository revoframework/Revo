using System.Threading.Tasks;

namespace Revo.Core.Transactions
{
    public interface ITransactionParticipant
    {
        Task OnBeforeCommitAsync();
        Task OnCommitSucceededAsync();
        Task OnCommitFailedAsync();
    }
}
