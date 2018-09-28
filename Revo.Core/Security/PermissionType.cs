using System;
using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Core.Security
{
    public class PermissionType : ValueObject<PermissionType>
    {
        public PermissionType(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Id), Id);
            yield return (nameof(Name), Name);
        }
    }
}
