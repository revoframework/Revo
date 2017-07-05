using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Core;
using GTRevo.Core.Lifecycle;
using GTRevo.Infrastructure.Domain.Attributes;

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

        public IEnumerable<Type> DomainEntities => typesToIds.Keys;

        public virtual void OnApplicationStarted()
        {
            EnsureLoaded();
        }

        protected virtual void EnsureLoaded()
        {
            if (typesToIds == null)
            {
                var entities = typeExplorer.GetAllTypes()
                    .Where(x => typeof(IEntity).IsAssignableFrom(x))
                    .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)
                    .ToList();

                typesToIds = entities.ToDictionary(x => x,
                    x => EntityClassUtils.TryGetEntityClassId(x));

                idsToTypes = typesToIds
                    .Where(x => x.Value.HasValue)
                    .ToDictionary(x => x.Value.Value, x => x.Key);
            }
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
        
        public Guid? TryGetClassIdByClrType(Type clrType)
        {
            EnsureLoaded();
            Guid? classId = null;
            typesToIds.TryGetValue(clrType, out classId);
            return classId;
        }

        public Type TryGetClrTypeByClassId(Guid classId)
        {
            EnsureLoaded();
            Type clrType = null;
            idsToTypes.TryGetValue(classId, out clrType);
            return clrType;
        }
    }
}
