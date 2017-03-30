using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GTRevo.Platform.Core
{
    public class AspNetTypeExplorer : TypeExplorer
    {
        public override IEnumerable<Assembly> GetAllReferencedAssemblies()
        {
            return System.Web.Compilation.BuildManager.GetReferencedAssemblies()
               .Cast<Assembly>()
               /*.Where(a => a.GetName().Name.StartsWith("System") == false)*/;
        }
    }
}
