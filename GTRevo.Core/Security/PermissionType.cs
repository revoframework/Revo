using System;

namespace GTRevo.Core.Security
{
    public class PermissionType
    {
        public PermissionType(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
    }
}
