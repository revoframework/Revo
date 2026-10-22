using System;

namespace Revo.Core.Types
{
    public class VersionedType(VersionedTypeId id, Type clrType)
    {
        public VersionedTypeId Id { get; } = id;
        public Type ClrType { get; } = clrType;

        public override string ToString() => $"VersionedType {{ Id = {Id}, ClrType = {ClrType.FullName} }}";

        public override bool Equals(object obj) =>
            obj is VersionedType other
                   && other.Id.Equals(Id)
                   && other.ClrType == ClrType;

        public override int GetHashCode() => (Id.GetHashCode() * 397) ^ ClrType.GetHashCode();
    }
}
