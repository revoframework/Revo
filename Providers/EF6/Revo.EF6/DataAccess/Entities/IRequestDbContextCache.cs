using System;
using System.Data.Entity;

namespace Revo.EF6.DataAccess.Entities
{
    public interface IRequestDbContextCache : IDisposable
    {
        void AddDbContext(DbContext dbContext);
    }
}
