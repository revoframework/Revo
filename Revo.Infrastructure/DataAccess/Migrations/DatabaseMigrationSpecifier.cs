using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationSpecifier : ValueObject<DatabaseMigrationSpecifier>
    {
        public DatabaseMigrationSpecifier(string moduleName, DatabaseVersion version)
        {
            ModuleName = moduleName;
            Version = version;
        }

        public string ModuleName { get; }
        public DatabaseVersion Version { get; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(ModuleName), ModuleName);
            yield return (nameof(Version), Version);
        }

        public override string ToString()
        {
            return $"{ModuleName}@{Version?.ToString() ?? "latest"}";
        }
    }
}