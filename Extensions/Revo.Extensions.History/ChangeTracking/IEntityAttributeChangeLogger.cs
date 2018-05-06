using System;
using System.Threading.Tasks;

namespace Revo.Extensions.History.ChangeTracking
{
    public interface IEntityAttributeChangeLogger
    {
        Task ChangeAttribute<T>(string attributeName, T newValue, Guid? aggregateId = null,
            Guid? aggregateClassId = null, Guid? entityId = null, Guid? entityClassId = null,
            Guid? userId = null);
        EntityAttributeChangeBatch Batch(Guid? aggregateId = null, Guid? aggregateClassId = null,
            Guid? entityId = null, Guid? entityClassId = null, Guid? userId = null);
    }
}