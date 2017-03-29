using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Domain
{
    public interface IHasId<TId>
    {
        TId Id { get; }
    }
}
