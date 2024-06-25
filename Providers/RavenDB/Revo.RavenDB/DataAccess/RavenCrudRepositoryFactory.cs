using Raven.Client.Documents.Session;
using Revo.DataAccess.Entities;

namespace Revo.RavenDB.DataAccess
{
    public class RavenCrudRepositoryFactory(IAsyncDocumentSession asyncDocumentSession) :
        ICrudRepositoryFactory<IRavenCrudRepository>,
        ICrudRepositoryFactory<ICrudRepository>,
        ICrudRepositoryFactory<IReadRepository>
    {
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
