using System;
using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Core.Security
{
    public class PermissionType(Guid id, string name) : ValueObject<PermissionType>
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Id), Id);
            yield return (nameof(Name), Name);
        }
    }
}
