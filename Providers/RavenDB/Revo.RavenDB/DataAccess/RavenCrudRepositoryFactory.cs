using Raven.Client.Documents.Session;
using Revo.DataAccess.Entities;

namespace Revo.RavenDB.DataAccess
{
    public class RavenCrudRepositoryFactory :
        ICrudRepositoryFactory<IRavenCrudRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IReadRepository>
    {
        private readonly IAsyncDocumentSession asyncDocumentSession;

        public RavenCrudRepositoryFactory(IAsyncDocumentSession asyncDocumentSession)
        {
            this.asyncDocumentSession = asyncDocumentSession;
        }

        public IRavenCrudRepository Create()
        {
            return new RavenCrudRepository(asyncDocumentSession);
        }

        ICrudRepository ICrudRepositoryFactory<ICrudRepository>.Create()
        {
            return Create();
        }

        IReadRepository ICrudRepositoryFactory<IReadRepository>.Create()
        {
            return Create();
        }
    }
}
