using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Revo.Core.ValueObjects;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationInfo : ValueObject<DatabaseMigrationInfo>
    {
        public DatabaseMigrationInfo(string moduleName, bool isBaseline, bool isRepeatable,
            DatabaseVersion version, ImmutableArray<ImmutableHashSet<string>> tags, string description, string checksum)
        {
            ModuleName = moduleName;
            IsBaseline = isBaseline;
            IsRepeatable = isRepeatable;
            Version = version;
            Tags = tags;
            Description = description;
            Checksum = checksum;
        }

        public DatabaseMigrationInfo(IDatabaseMigration migration)
        {
            ModuleName = migration.ModuleName;
            IsBaseline = migration.IsBaseline;
            IsRepeatable = migration.IsRepeatable;
            Version = migration.Version;
            Tags = migration.Tags.Select(x => x.ToImmutableHashSet()).ToImmutableArray();
            Description = migration.Description;
            Checksum = migration.Checksum;
        }

        public string ModuleName { get; }
        public bool IsBaseline { get; }
        public bool IsRepeatable { get; }
        public DatabaseVersion Version { get; }
        public ImmutableArray<ImmutableHashSet<string>> Tags { get; }
        public string Description { get; }
        public string Checksum { get; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(ModuleName), ModuleName);
            yield return (nameof(IsBaseline), IsBaseline);
            yield return (nameof(IsRepeatable), IsRepeatable);
            yield return (nameof(Version), Version);
            yield return (nameof(Tags), Tags);
            yield return (nameof(Description), Description);
            yield return (nameof(Checksum), Checksum);
        }
    }
}