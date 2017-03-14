using System;
using System.Threading.Tasks;

namespace GTRevo.Platform.Transactions
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        Task CommitAsync();
    }
}
