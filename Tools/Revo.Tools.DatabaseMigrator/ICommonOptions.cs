using System.Collections.Generic;
using CommandLine;

namespace Revo.Tools.DatabaseMigrator
{
    public interface ICommonOptions
    {
        [Option('c', "connectionString", Required = true, HelpText = "Connection string for the database provider.")]
        string ConnectionString { get; set; }

        [Option('p', "databaseProvider", Required = true, HelpText = "Used database provider. Currently supported values are: Npgsql, SqlServer, SQLite.")]
        DatabaseProvider DatabaseProvider { get; set; }

        [Option('a', "assemblies", Required = false, HelpText = "Assemblies containing NinjectModules to load. Migrator will execute all migrations registered in these modules.")]
        IEnumerable<string> Assemblies  { get; set; }

        [Option('d', "directoryPaths", Required = false, HelpText = "Directory paths to recursively search for migration files. This is not needed when 'assemblies' are specified. Finds any files matching the 'fileNameRegex'.")]
        IEnumerable<string> DirectoryPaths { get; set; }

        [Option('f', "fileNameRegex", Required = false, HelpText = @"Regex for matching the names of files found when specifying 'directoryPaths'. Default is: ^(?<module>[a-z0-9\-]+)(?:_(?<baseline>baseline))?(?:_(?<repeatable>repeatable))?(?:_(?<version>\d+(?:\.\d+)*))?(?:_(?<tag>[a-z0-9\-]+)(?:,(?<tag>[a-z0-9\-]+))*)?\.sql$")]
        string FileNameRegex { get; set; }

        [Option('m', "modules", Required = false, HelpText = "Names of modules to migrate. If empty, will migrate all found modules. May also specify the target version of every module and/or use wildcards like this: 'mycompany-*@1.2.3'.;")]
        IEnumerable<string> Modules { get; set; }

        [Option('e', "environmentTags", Required = false, HelpText = "Additional environment tags.")]
        IEnumerable<string> EnvironmentTags { get; set; }
    }
}