using System;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public interface ISleep
    {
        void Sleep(TimeSpan timeout);
        Task SleepAsync(TimeSpan timeout);
    }
}
