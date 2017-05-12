using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.ReadModel
{
    [DatabaseEntity(SchemaSpace = "ReadModel")]
    public abstract class EntityView : IHasId<Guid>
    {
        public Guid Id { get; set; }
    }
}
