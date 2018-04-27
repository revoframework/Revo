using System;
using System.Collections.Generic;
using System.Configuration;
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
using Revo.Core.IO.OData;
using Revo.DataAccess.RavenDB.IO.OData;
using Revo.Platforms.AspNet.Core.Lifecycle;

namespace Revo.DataAccess.RavenDB
{
    public class RavenModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDocumentStore>()
                .ToMethod(ctx =>
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["RavenDB"].ConnectionString;
                    Dictionary<string, string> connectionParams = connectionString.Split(';')
                        .Select(value => value.Split('='))
                        .ToDictionary(pair => pair[0].Trim(), pair => pair.Length > 0 ? pair[1].Trim() : null);

                    var store = new DocumentStore()
                    {
                        Urls = new []
                        {
                            connectionParams.TryGetValue("Url", out string url) ? url : "http://localhost:8998"
                        },
                        Database = connectionParams.TryGetValue("Database", out string database) ? database : "Revo"
                    };

                    store.Conventions.FindIdentityProperty = memberInfo => memberInfo.Name == "DocumentId";
                    store.Conventions.UseOptimisticConcurrency = true;

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

            Bind<IQueryableToODataResultConverter>()
                .To<RavenQueryableToODataResultConverter>()
                .InSingletonScope();
        }
    }
}
