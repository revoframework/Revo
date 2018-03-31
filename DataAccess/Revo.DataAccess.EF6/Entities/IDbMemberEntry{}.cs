using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6.Entities
{
    public interface IDbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        string Name { get; }
        TProperty CurrentValue { get; set; }
    }
}
