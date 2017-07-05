using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain.Attributes;

namespace GTRevo.Infrastructure.Domain
{
    public static class EntityClassUtils
    {
        public static Guid? TryGetEntityClassId<T>() where T : class
        {
            return TryGetEntityClassId(typeof(T));
        }

        public static Guid? TryGetEntityClassId(Type type)
        {
            DomainClassIdAttribute idAttribute =
                (DomainClassIdAttribute) type.GetCustomAttributes(typeof(DomainClassIdAttribute), false)
                    .FirstOrDefault();
            return idAttribute?.ClassId;
        }

        public static Guid GetEntityClassId<T>() where T : class
        {
            return GetEntityClassId(typeof(T));
        }

        public static Guid GetEntityClassId(Type type)
        {
            Guid? classId = TryGetEntityClassId(type);
            if (classId == null)
            {
                throw new ArgumentException($"Type {type.FullName} doesn't have an attribute with class ID");
            }

            return classId.Value;
        }
    }
}
