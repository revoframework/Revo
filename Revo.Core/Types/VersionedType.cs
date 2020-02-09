using System;

namespace Revo.Core.Types
{
    public class VersionedType
    {
        public VersionedType(VersionedTypeId id, Type clrType)
        {
            Id = id;
            ClrType = clrType;
        }

        public VersionedTypeId Id { get; }
        public Type ClrType { get; }

        public override string ToString()
        {
            return $"VersionedType {{ Id = {Id}, ClrType = {ClrType.FullName} }}";
        }

        public override bool Equals(object obj)
        {
            return obj is VersionedType other
                   && other.Id.Equals(Id)
                   && other.ClrType == ClrType;
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 397) ^ ClrType.GetHashCode();
        }
    }
}
