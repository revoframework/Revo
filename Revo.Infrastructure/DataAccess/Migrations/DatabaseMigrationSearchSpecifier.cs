using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    /// <summary>
    /// Specifies a database migration wildcard.
    /// </summary>
    public class DatabaseMigrationSearchSpecifier : ValueObject<DatabaseMigrationSearchSpecifier>
    {
        public DatabaseMigrationSearchSpecifier(string moduleNameWildcard, DatabaseVersion version)
        {
            ModuleNameWildcard = moduleNameWildcard;
            Version = version;
        }

        /// <summary>
        /// Wildcard that is used to match migration module names. You can use '*' (any characters)
        /// and '?' (at least one character) jokers, e.g. 'myapp-*'.
        /// </summary>
        public string ModuleNameWildcard { get; }

        /// <summary>
        /// Module target version. All migrations matching ModuleNameWildcard are migrated to this Version.
        /// </summary>
        public DatabaseVersion Version { get; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(ModuleNameWildcard), ModuleNameWildcard);
            yield return (nameof(Version), Version);
        }
    }
}