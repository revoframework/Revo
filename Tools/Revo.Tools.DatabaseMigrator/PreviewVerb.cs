using System.Collections.Generic;
using CommandLine;

namespace Revo.Tools.DatabaseMigrator
{
    [Verb("preview", HelpText = "Upgrades the database to newer version.")]
    public class PreviewVerb : ICommonOptions
    {
        public string ConnectionString { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; }
        public IEnumerable<string> Assemblies { get; set; }
        public IEnumerable<string> DirectoryPaths { get; set; }
        public string FileNameRegex { get; set; }
        public IEnumerable<string> Modules { get; set; }
        public IEnumerable<string> EnvironmentTags { get; set; }
    }
}