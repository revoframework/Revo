using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IEventSourceCatchUp
    {
        Task CatchUpAsync();
    }
}
