using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Domain.ReadModel
{
    public interface IClassEntityReadModel : IEntityReadModel
    {
        Guid ClassId { get; set; }
    }
}
