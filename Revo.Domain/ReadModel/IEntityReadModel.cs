using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Domain.Core;

namespace Revo.Domain.ReadModel
{
    public interface IEntityReadModel : IHasId<Guid>
    {
        new Guid Id { get; set; }
    }
}
