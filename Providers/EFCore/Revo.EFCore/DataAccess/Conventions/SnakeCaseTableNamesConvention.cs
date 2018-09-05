using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class SnakeCaseTableNamesConvention : EFCoreConventionBase
    {

        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = ToSnakeCase(entity.Relational().TableName);
            }
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}
