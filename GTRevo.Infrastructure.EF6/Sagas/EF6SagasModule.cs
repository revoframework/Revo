using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Infrastructure.Sagas;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.Sagas
{
    public class EF6SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaMetadataRepository>()
                .To<EF6SagaMetadataRepository>()
                .InRequestOrJobScope();
        }
    }
}
