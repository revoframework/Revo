using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Core.Domain.Basic
{
    public interface IBasicClassIdEntity
    {
        Guid ClassId { get; set; }
    }
}
