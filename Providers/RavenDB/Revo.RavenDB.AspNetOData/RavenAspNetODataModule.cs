using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Revo.AspNet.IO.OData;
using Revo.RavenDB.AspNetOData.IO.OData;

namespace Revo.RavenDB.AspNetOData
{
    public class RavenAspNetODataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IQueryableToODataResultConverter>()
                .To<RavenQueryableToODataResultConverter>()
                .InSingletonScope();
        }
    }
}
