using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.History
{
    public interface IChangeTracker
    {
        void AddChanges<T>(Guid? aggregateId, Guid? aggregateClassId,
            Guid? entityId, Guid? entityClassId, T changeData);
    }
}
