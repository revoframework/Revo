using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.DataAccess.RavenDB.Entities;
using Ninject;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Revo.DataAccess.RavenDB
{
    public class RavenModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDocumentStore>()
                .ToMethod(ctx =>
                {
                    var store = new DocumentStore()
                    {
                        Urls = new []
                        {
                            "http://192.168.2.111:8998"
                        },
                        Database = "DEV_AnyBounty"
                        // = "RavenDB"
                    };

                    store.Conventions.FindIdentityProperty = memberInfo => memberInfo.Name == "DocumentId";

                    return store.Initialize();
                })
                .InSingletonScope();

            Bind<IAsyncDocumentSession>()
                .ToMethod(ctx =>
                {
                    var documentStore = ctx.Kernel.Get<IDocumentStore>();
                    return documentStore.OpenAsyncSession();
                })
                .InRequestOrJobScope();

            Bind<IRavenCrudRepository>()
                .To<RavenCrudRepository>()
                .InRequestOrJobScope();
        }
    }
}
