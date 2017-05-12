using System.Threading.Tasks;

namespace GTRevo.Platform.Transactions
{
    public interface ITransactionListener
    {
        void OnTransactionBegin(ITransaction transaction);
        Task OnTransactionSucceededAsync(ITransaction transaction);
    }
}
