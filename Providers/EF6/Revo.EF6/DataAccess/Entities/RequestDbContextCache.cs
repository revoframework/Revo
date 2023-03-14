using System.Collections.Generic;
using System.Data.Entity;

namespace Revo.EF6.DataAccess.Entities
{
    public class RequestDbContextCache : IRequestDbContextCache
    {
        private readonly HashSet<DbContext> dbContexts = new HashSet<DbContext>();

        public void AddDbContext(DbContext dbContext)
        {
            dbContexts.Add(dbContext);
        }

        public void Dispose()
        {
            foreach (var dbContext in dbContexts)
            {
                dbContext.Dispose();
            }
        }
    }
}
