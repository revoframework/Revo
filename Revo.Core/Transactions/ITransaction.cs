using System;
using System.Threading.Tasks;

namespace Revo.Core.Transactions
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        Task CommitAsync();
    }
}
