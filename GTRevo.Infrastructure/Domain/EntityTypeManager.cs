using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Infrastructure.Domain.Attributes;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Infrastructure.Domain
{
    public class EntityTypeManager : IEntityTypeManager, IApplicationStartListener
    {
        private readonly ITypeExplorer typeExplorer;
        private Dictionary<Type, Guid?> typesToIds;
        private Dictionary<Guid, Type> idsToTypes;

        public EntityTypeManager(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }

        public IEnumerable<Type> DomainEntities
        {
            get
            {
                return typesToIds.Keys;
            }
        }

        public virtual void OnApplicationStarted()
        {
            var entities = typeExplorer.GetAllTypes()
                .Where(x => typeof(IEntity).IsAssignableFrom(x))
                .ToList();

            typesToIds = entities.ToDictionary(x => x,
                x =>
                    ((DomainClassIdAttribute)
                        x.GetCustomAttributes(typeof(DomainClassIdAttribute), false).FirstOrDefault())?.ClassId);

            idsToTypes = typesToIds
                .Where(x => x.Value.HasValue)
                .ToDictionary(x => x.Value.Value, x => x.Key);
        }

        public virtual Guid GetClassIdByClrType(Type clrType)
        {
            Guid? classId = TryGetClassIdByClrType(clrType);
            if (classId == null)
            {
                throw new ArgumentException("Domain class ID not found for CLR type: " + clrType.FullName);
            }

            return classId.Value;
        }

        public virtual Type GetClrTypeByClassId(Guid classId)
        {
            Type clrType = TryGetClrTypeByClassId(classId);
            if (clrType == null)
            {
                throw new ArgumentException("Domain class not found for class ID: " + classId);
            }

            return clrType;
        }
        
        protected Guid? TryGetClassIdByClrType(Type clrType)
        {
            Guid? classId = null;
            typesToIds.TryGetValue(clrType, out classId);
            return classId;
        }

        protected Type TryGetClrTypeByClassId(Guid classId)
        {
            Type clrType = null;
            idsToTypes.TryGetValue(classId, out clrType);
            return clrType;
        }
    }
}
