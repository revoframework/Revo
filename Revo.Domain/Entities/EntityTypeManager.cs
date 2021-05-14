using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Domain.Entities
{
    public class EntityTypeManager : IEntityTypeManager, IApplicationStartedListener
    {
        private readonly ITypeExplorer typeExplorer;
        private Lazy<Dictionary<Type, DomainClassInfo>> typesToClassIds;
        private Lazy<Dictionary<Guid, DomainClassInfo>> idsToTypes;

        public EntityTypeManager(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
            ClearCache();
        }

        public IEnumerable<DomainClassInfo> DomainEntities => typesToClassIds.Value.Values;

        public virtual void OnApplicationStarted()
        {
            ClearCache();
        }

        public void ClearCache()
        {
            typesToClassIds = new Lazy<Dictionary<Type, DomainClassInfo>>(() =>
            {
                var entities = typeExplorer.GetAllTypes()
                    .Where(x => typeof(IEntity).IsAssignableFrom(x))
                    .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition && !x.IsConstructedGenericType)
                    .Select(x => new { Type = x, ClassIdAttribute = EntityClassUtils.GetClassIdAttribute(x) })
                    .Where(x => x.ClassIdAttribute != null)
                    .ToList();

                return entities.ToDictionary(x => x.Type,
                    x =>
                    {
                        return new DomainClassInfo(x.ClassIdAttribute.ClassId, x.ClassIdAttribute.Code, x.Type);
                    });
            });

            idsToTypes = new Lazy<Dictionary<Guid, DomainClassInfo>>(() =>
            {
                return typesToClassIds.Value
                    .ToDictionary(x => x.Value.Id, x => x.Value);
            });
        }

        public virtual DomainClassInfo GetClassInfoByClrType(Type clrType)
        {
            DomainClassInfo classInfo = TryGetClassInfoByClrType(clrType);
            if (classInfo == null)
            {
                throw new ArgumentException("Domain class info not found for CLR type: " + clrType.FullName);
            }

            return classInfo;
        }

        public virtual DomainClassInfo GetClassInfoByClassId(Guid classId)
        {
            DomainClassInfo classInfo = TryGetClassInfoByClassId(classId);
            if (classInfo == null)
            {
                throw new ArgumentException("Domain class not found for class ID: " + classId);
            }

            return classInfo;
        }
        
        public DomainClassInfo TryGetClassInfoByClrType(Type clrType)
        {
            typesToClassIds.Value.TryGetValue(clrType, out DomainClassInfo res);
            return res;
        }

        public DomainClassInfo TryGetClassInfoByClassId(Guid classId)
        {
            idsToTypes.Value.TryGetValue(classId, out var classInfo);
            return classInfo;
        }
    }
}
