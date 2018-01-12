using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.Entities
{
    public interface IManuallyRowVersioned
    {
        int Version { get; set; }
    }
}
