using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Revo.AspNet.IO.OData;
using Revo.EF6.AspNetOData.IO.OData;

namespace Revo.EF6.AspNetOData
{
    public class EF6AspNetODataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IQueryableToODataResultConverter>()
                .To<EF6QueryableToODataResultConverter>()
                .InSingletonScope();
        }
    }
}
