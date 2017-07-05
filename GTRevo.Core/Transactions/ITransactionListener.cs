using System.Threading.Tasks;

namespace GTRevo.Core.Transactions
{
    public interface ITransactionListener
    {
        void OnTransactionBegin(ITransaction transaction);
        Task OnTransactionSucceededAsync(ITransaction transaction);
    }
}
