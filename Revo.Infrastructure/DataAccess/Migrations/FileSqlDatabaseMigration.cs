using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public abstract class FileSqlDatabaseMigration : SqlDatabaseMigration
    {
        public static readonly Regex DefaultFileNameRegex = new Regex(@"^(?<module>[a-z0-9\-]+)(?:_(?<baseline>baseline))?(?:_(?<repeatable>repeatable))?(?:_(?<version>\d+(?:\.\d+)*))?(?:_(?<tag>[a-z0-9\-]+)(?:,(?<tag>[a-z0-9\-]+))*)?\.sql$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string moduleName;
        private DatabaseVersion version;
        private string[][] tags;
        public DatabaseMigrationTransactionMode transactionMode = DatabaseMigrationTransactionMode.Default;
        private bool isBaseline;
        private bool isRepeatable;
        private string[] sqlCommands;
        private string description;
        private List<DatabaseMigrationSpecifier> dependencies = new List<DatabaseMigrationSpecifier>();
        private bool hasParsedFile;
        
        protected FileSqlDatabaseMigration(string fileName, Regex fileNameRegex = null)
        {
            FileName = fileName;
            FileNameRegex = fileNameRegex ?? DefaultFileNameRegex;
            ParseFileName();
        }

        public Regex FileNameRegex { get; }

        private void ParseFileName()
        {
            var match = FileNameRegex.Match(FileName);
            if (!match.Success)
            {
                throw new FormatException($"Invalid FileSqlDatabaseMigration file name: must match the following regex '{FileNameRegex}'");
            }

            moduleName = match.Groups["module"].Captures.Count > 0
                ? match.Groups["module"].Captures[0].Value
                : throw new FormatException($"Invalid FileSqlDatabaseMigration file name: missing module name in '{FileName}'");
            
            isBaseline = match.Groups["baseline"].Captures.Count > 0;
            isRepeatable = match.Groups["repeatable"].Captures.Count > 0;

            version = match.Groups["version"].Captures.Count > 0
                ? DatabaseVersion.Parse(match.Groups["version"].Captures[0].Value) : null;

            tags = match.Groups["tag"].Captures.Count > 0
                ? new[] { match.Groups["tag"].Captures.OfType<Capture>().Select(x => x.Value).ToArray() }
                : new string[0][];
        }

        public string FileName { get; private set; }

        public override DatabaseVersion Version
        {
            get
            {
                if (version == null && !IsRepeatable)
                {
                    if (!hasParsedFile)
                    {
                        ParseFile();
                    }

                    if (version == null)
                    {
                        throw new FormatException($"Invalid FileSqlDatabaseMigration: {FileName} is missing version");
                    }
                }

                return version;
            }
        }

        public override string ModuleName => moduleName;
        public override string[][] Tags => tags;
        public override bool IsBaseline => isBaseline;
        public override bool IsRepeatable => isRepeatable;

        public override string Description

        {
            get
            {
                if (!hasParsedFile)
                {
                    ParseFile();
                }

                return description;
            }
        }

        public override DatabaseMigrationTransactionMode TransactionMode
        {
            get
            {
                if (!hasParsedFile)
                {
                    ParseFile();
                }

                return transactionMode;
            }
        }

        public override IReadOnlyCollection<DatabaseMigrationSpecifier> Dependencies

        {
            get
            {
                if (!hasParsedFile)
                {
                    ParseFile();
                }

                return dependencies;
            }
        }


        public override string[] SqlCommands
        {
            get
            {
                if (!hasParsedFile)
                {
                    ParseFile();
                }

                return sqlCommands;
            }
        }

        public override string ToString(bool includeClassName)
        {
            if (includeClassName)
            {
                return base.ToString(true);
            }
            else
            {
                return $"{base.ToString(false)} from {FileName}";
            }
        }

        protected abstract string ReadSqlFileContents();

        protected void ParseFile()
        {
            string contents = ReadSqlFileContents();
            sqlCommands = new[] {contents};

            int i = 0;
            while (i < contents.Length)
            {
                if (char.IsWhiteSpace(contents[i]))
                {
                    i++;
                }
                else if (contents[i] == '-' && i + 2 < contents.Length && contents[i + 1] == '-')
                {
                    int eol = contents.IndexOfAny(new[] {'\r', '\n'}, i + 2);
                    if (eol == -1)
                    {
                        break;
                    }

                    string line = contents.Substring(i + 2, eol - i - 2);
                    line = line.Trim();

                    ParseLine(line);
                    i = eol + 1;
                }
                else if (contents[i] == '/' && i + 4 < contents.Length && contents[i + 1] == '*')
                {
                    int end = contents.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (end == -1)
                    {
                        break;
                    }

                    i = end + 2;
                }
                else
                {
                    break;
                }
            }

            hasParsedFile = true;
        }

        private void ParseLine(string line)
        {
            if (line.StartsWith("description:"))
            {
                line = line.Substring("description:".Length).Trim();
                description = line;
            }
            else if (line.StartsWith("version:"))
            {
                line = line.Substring("version:".Length).Trim();
                var newVersion = DatabaseVersion.Parse(line);

                if (version != null && !Equals(version, newVersion))
                {
                    throw new FormatException($"Version specified in {this} headers ({newVersion}) is different from version specified in its file name ({version})");
                }

                version = newVersion;
            }
            else if (line.StartsWith("dependency:"))
            {
                line = line.Substring("dependency:".Length).Trim();
                int at = line.IndexOf('@');
                if (at == -1)
                {
                    dependencies.Add(new DatabaseMigrationSpecifier(line, null));
                }
                else
                {
                    string moduleName = line.Substring(0, at);
                    string versionString = line.Substring(at + 1);
                    dependencies.Add(new DatabaseMigrationSpecifier(moduleName, DatabaseVersion.Parse(versionString)));
                }
            }
            else if (line.StartsWith("transactionMode:"))
            {
                line = line.Substring("transactionMode:".Length).Trim().ToLowerInvariant();
                switch (line)
                {
                    case "default":
                        transactionMode = DatabaseMigrationTransactionMode.Default;
                        break;

                    case "isolated":
                        transactionMode = DatabaseMigrationTransactionMode.Isolated;
                        break;

                    case "withouttransaction":
                        transactionMode = DatabaseMigrationTransactionMode.WithoutTransaction;
                        break;

                    default:
                        throw new FormatException($"Unknown value '{line}' for transactionMode in {this}");
                }
            }
        }
    }
}