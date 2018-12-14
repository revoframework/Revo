using System;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Domain;
using Revo.EFCore.Projections;
using Revo.EFCore.UnitOfWork;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore
{
    public class EFCoreInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAggregateStoreFactory>()
                .To<EFCoreCrudAggregateStoreFactory>()
                .InTransientScope();

            Bind<IAggregateStoreFactory>()
                .To<EFCoreEventSourcedAggregateStoreFactory>()
                .InTransientScope();

            Bind<IEFCoreTransactionCoordinator, IUnitOfWorkListener>()
                .ToMethod(ctx => new EFCoreCoordinatedTransaction(ctx.ContextPreservingGet<IEFCoreCrudRepository>(),
                    ctx.ContextPreservingGet<ICommandContext>(),
                    ctx.ContextPreservingGet<Lazy<EFCoreExternalEventStoreHook>>(),
                    ctx.ContextPreservingGet<Lazy<EFCoreSyncProjectionHook>>()))
                .InTaskScope();
        }
    }
}
