using System.Threading.Tasks;

namespace GTRevo.Platform.Transactions
{
    public interface ITransactionListener
    {
        void OnTransactionBeginned(ITransaction transaction);
        Task OnTransactionSucceededAsync(ITransaction transaction);
    }
}
