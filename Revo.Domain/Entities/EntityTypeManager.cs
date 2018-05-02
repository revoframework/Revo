using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Domain.Entities.Attributes;

namespace Revo.Domain.Entities
{
    public class EntityTypeManager : IEntityTypeManager, IApplicationStartListener
    {
        private readonly ITypeExplorer typeExplorer;
        private Dictionary<Type, DomainClassIdAttribute> typesToClassIds;
        private Dictionary<Guid, Type> idsToTypes;

        public EntityTypeManager(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }

        public IEnumerable<Type> DomainEntities => typesToClassIds.Keys;

        public virtual void OnApplicationStarted()
        {
            EnsureLoaded();
        }

        protected virtual void EnsureLoaded()
        {
            if (typesToClassIds == null)
            {
                var entities = typeExplorer.GetAllTypes()
                    .Where(x => typeof(IEntity).IsAssignableFrom(x))
                    .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)
                    .ToList();

                typesToClassIds = entities.ToDictionary(x => x,
                    EntityClassUtils.GetClassIdAttribute);

                idsToTypes = typesToClassIds
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Value.ClassId, x => x.Key);
            }
        }

        public virtual DomainClassIdAttribute GetClassIdByClrType(Type clrType)
        {
            var res = TryGetClassIdByClrType(clrType);
            if (res == null)
            {
                throw new ArgumentException("Domain class ID not found for CLR type: " + clrType.FullName);
            }

            return res;
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
        
        public DomainClassIdAttribute TryGetClassIdByClrType(Type clrType)
        {
            EnsureLoaded();
            typesToClassIds.TryGetValue(clrType, out DomainClassIdAttribute res);
            return res;
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
