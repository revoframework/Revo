using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Jobs
{
    public interface IJobRunner
    {
        Task RunJobAsync(IJob job);
    }
}
