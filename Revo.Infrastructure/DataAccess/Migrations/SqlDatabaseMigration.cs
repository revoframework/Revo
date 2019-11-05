using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public abstract class SqlDatabaseMigration : ISqlDatabaseMigration
    {
        public abstract string ModuleName { get; }
        public abstract DatabaseVersion Version { get; }
        public virtual bool IsBaseline => false;
        public virtual bool IsRepeatable => false;
        public virtual IReadOnlyCollection<DatabaseMigrationSpecifier> Dependencies => new DatabaseMigrationSpecifier[0];
        public virtual string[][] Tags => new string[0][];
        public virtual string Description => null;

        public string Checksum
        {
            get
            {
                string input = string.Join("\n\n", SqlCommands);

                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
        }

        public abstract string[] SqlCommands { get; }

        public override string ToString()
        {
            return $"{nameof(SqlDatabaseMigration)} {{{ModuleName}@{Version}}}";
        }
    }
}