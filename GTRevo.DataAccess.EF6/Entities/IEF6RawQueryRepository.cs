using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6.Entities
{
    public interface IEF6RawQueryRepository: IEF6CrudRepository
    {
        DbContext GetDbContext(Type entityType);
    }
}
