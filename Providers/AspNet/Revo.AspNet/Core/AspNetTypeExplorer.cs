using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Revo.Core.Types;

namespace Revo.AspNet.Core
{
    public class AspNetTypeExplorer : TypeExplorer
    {
        public override IEnumerable<Assembly> GetAllReferencedAssemblies()
        {
            return System.Web.Compilation.BuildManager.GetReferencedAssemblies()
               .Cast<Assembly>();
        }
    }
}
