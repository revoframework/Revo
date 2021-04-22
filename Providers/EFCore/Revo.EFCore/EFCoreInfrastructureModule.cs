using System;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Transactions;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Projections;
using Revo.EFCore.Repositories;

namespace Revo.EFCore
{
    public class EFCoreInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEFCoreTransactionCoordinator, IUnitOfWorkListener>()
                .ToMethod(ctx => new EFCoreCoordinatedTransaction(ctx.ContextPreservingGet<IEFCoreCrudRepository>(),
                    ctx.ContextPreservingGet<ICommandContext>(),
                    ctx.ContextPreservingGet<Lazy<EFCoreSyncProjectionHook>>()))
                .InTaskScope();
        }
    }
}
