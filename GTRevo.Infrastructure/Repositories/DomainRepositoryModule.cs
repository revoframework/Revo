using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
using GTRevo.Platform.Core;
using GTRevo.Platform.Transactions;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Repositories
{
    public class DomainRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository, IUnitOfWorkProvider>()
                .To<Repository>()
                .InRequestOrJobScope();

            Bind<ICrudRepository>()
                .To<CrudRepository>()
                .WhenInjectedInto<EF6AggregateStore>()
                .InTransientScope();

            Bind<IAggregateStore>()
                .To<EF6AggregateStore>()
                .InTransientScope();

            Bind<IAggregateStore>()
                .To<EventSourcedAggregateStore>()
                .InTransientScope();
        }
    }
}
