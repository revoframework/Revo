using System;
using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Entities
{
    public interface IRequestDbContextCache : IDisposable
    {
        void AddDbContext(DbContext dbContext);
    }
}
