using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;

namespace GTRevo.DataAccess.EF6.Entities
{
    public interface IEF6CrudRepository : IEF6ReadRepository, ICrudRepository
    {
    }
}
