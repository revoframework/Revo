using System.Threading.Tasks;

namespace Revo.Core.Transactions
{
    public interface ITransactionListener
    {
        void OnTransactionBegin(ITransaction transaction);
        Task OnTransactionSucceededAsync(ITransaction transaction);
    }
}
