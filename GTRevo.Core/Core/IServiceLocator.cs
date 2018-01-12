using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Core.Core
{
    public interface IServiceLocator
    {
        object Get(Type serviceType);
        IEnumerable<object> GetAll(Type serviceType);
    }
}
