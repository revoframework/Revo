using System;

namespace Revo.Core.Security
{
    public class PermissionType
    {
        public PermissionType(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }

        public override bool Equals(object obj)
        {
            PermissionType other = obj as PermissionType;
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"PermissionType {{ Id = {Id}, Name = {Name} }}";
        }
    }
}
