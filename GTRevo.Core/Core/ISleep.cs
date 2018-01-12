using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Core.Core
{
    public interface ISleep
    {
        void Sleep(TimeSpan timeout);
        Task SleepAsync(TimeSpan timeout);
    }
}
