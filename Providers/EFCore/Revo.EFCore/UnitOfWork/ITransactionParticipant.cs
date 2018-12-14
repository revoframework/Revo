using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Revo.EFCore.UnitOfWork
{
    public interface ITransactionParticipant
    {
        Task OnBeforeCommitAsync();
        Task OnCommitSucceededAsync();
        Task OnCommitFailedAsync();
    }
}
