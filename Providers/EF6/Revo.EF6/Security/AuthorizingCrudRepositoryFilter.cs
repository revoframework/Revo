using System.Linq;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Security.Commands;

namespace Revo.EF6.Security
{
    public class AuthorizingCrudRepositoryFilter : IRepositoryFilter
    {
        private readonly IEntityQueryAuthorizer entityQueryAuthorizer;

        public AuthorizingCrudRepositoryFilter(IEntityQueryAuthorizer entityQueryAuthorizer)
        {
            this.entityQueryAuthorizer = entityQueryAuthorizer;
        }

        public IQueryable<T> FilterResults<T>(IQueryable<T> results) where T : class
        {
            return InjectQueryable(results);
        }

        public T FilterResult<T>(T result) where T : class
        {
            return result;
        }

        public void FilterAdded<T>(T added) where T : class
        {
        }

        public void FilterDeleted<T>(T deleted) where T : class
        {
        }

        public void FilterModified<T>(T modified) where T : class
        {
        }

        private IQueryable<T> InjectQueryable<T>(IQueryable<T> query)
        {
            var queryProvider = new AuthorizingQueryProvider(query.Provider, entityQueryAuthorizer);
            return queryProvider.InjectQueryable(query);
        }
    }
}
