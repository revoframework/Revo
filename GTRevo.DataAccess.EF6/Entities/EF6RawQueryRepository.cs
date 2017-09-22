using GTRevo.DataAccess.EF6.Model;
using GTRevo.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6.Entities
{
    public class EF6RawQueryRepository : EF6CrudRepository, IEF6RawQueryRepository
    {
        public EF6RawQueryRepository(IModelMetadataExplorer modelMetadataExplorer,
            IDbContextFactory dbContextFactory, IRepositoryFilter[] repositoryFilters): base(modelMetadataExplorer, dbContextFactory, repositoryFilters)
        {

        }

        DbContext IEF6RawQueryRepository.GetDbContext(Type entityType)
        {
            return base.GetDbContext(entityType);
        }
    }
}
