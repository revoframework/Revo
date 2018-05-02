using System;
using System.Linq;
using Revo.Domain.Entities.Attributes;

namespace Revo.Domain.Entities
{
    public static class EntityClassUtils
    {
        public static Guid? TryGetEntityClassId<T>() where T : class
        {
            return TryGetEntityClassId(typeof(T));
        }

        public static Guid? TryGetEntityClassId(Type type)
        {
            var idAttribute = GetClassIdAttribute(type);
            return idAttribute?.ClassId;
        }

        public static DomainClassIdAttribute GetClassIdAttribute(Type type)
        {
            DomainClassIdAttribute idAttribute =
                (DomainClassIdAttribute)type.GetCustomAttributes(typeof(DomainClassIdAttribute), false)
                    .FirstOrDefault();
            return idAttribute;
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
                throw new ArgumentException($"ClrType {type.FullName} doesn't have an attribute with class ID");
            }

            return classId.Value;
        }

        /// <summary>
        /// Gets  domain classid (GTClassId) for type.
        /// </summary>
        /// <param name="type">Examined type.</param>
        /// <returns>Domain classid.</returns>
        public static Guid GetClassId(this Type type)
        {
            return GetEntityClassId(type);
        }
    }
}
