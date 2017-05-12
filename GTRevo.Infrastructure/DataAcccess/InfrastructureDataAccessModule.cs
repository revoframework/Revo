using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;

namespace GTRevo.Infrastructure.DataAcccess
{
    public class InfrastructureDataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IConvention>()
               .To<CustomStoreConventionEx>()
               .InTransientScope();
        }
    }
}
