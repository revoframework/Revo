using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Jobs
{
    public interface IJobHandler<in T>
        where T : IJob
    {
        Task HandleAsync(T job, CancellationToken cancellationToken);
    }
}
