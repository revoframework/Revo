using System;
using System.Threading.Tasks;

namespace GTRevo.Transactions
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        Task CommitAsync();
    }
}
