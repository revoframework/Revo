using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.Modules;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.RavenDB.Configuration;
using Revo.RavenDB.DataAccess;

namespace Revo.RavenDB
{
    [AutoLoadModule(false)]
    public class RavenModule : NinjectModule
    {
        private readonly RavenConnectionConfiguration connectionConfiguration;
        private readonly bool useAsPrimaryRepository;

        public RavenModule(RavenConnectionConfiguration connectionConfiguration, bool useAsPrimaryRepository)
        {
            this.connectionConfiguration = connectionConfiguration;
            this.useAsPrimaryRepository = useAsPrimaryRepository;
        }

        public override void Load()
        {
            Bind<IDocumentStore>()
                .ToMethod(ctx =>
                {
                    Dictionary<string, string> connectionParams = connectionConfiguration?.ConnectionString?.Split(';')
                        .Select(value => value.Split('='))
                        .ToDictionary(pair => pair[0].Trim(), pair => pair.Length > 0 ? pair[1].Trim() : null);

                    var store = new DocumentStore()
                    {
                        Urls = new []
                        {
                            connectionParams != null && connectionParams.TryGetValue("Url", out string url) ? url : "http://localhost:8998"
                        },
                        Database = connectionParams != null && connectionParams.TryGetValue("Database", out string database) ? database : "Revo"
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
                .InTaskScope();

            List<Type> repositoryTypes = new List<Type>()
            {
                typeof(IRavenCrudRepository)
            };

            if (useAsPrimaryRepository)
            {
                repositoryTypes.AddRange(new[]
                {
                    typeof(ICrudRepository), typeof(IReadRepository)
                });
            }
            
            Bind(repositoryTypes.ToArray())
                .To<RavenCrudRepository>()
                .InTaskScope();

            Bind(repositoryTypes.Select(x => typeof(ICrudRepositoryFactory<>).MakeGenericType(x)).ToArray())
                .To<RavenCrudRepositoryFactory>()
                .InTaskScope();
        }
    }
}
