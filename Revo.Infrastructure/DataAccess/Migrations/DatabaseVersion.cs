using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Revo.Core.ValueObjects;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseVersion : ValueObject<DatabaseVersion>, IComparable<DatabaseVersion>
    {
        public DatabaseVersion(params int[] fractions)
        {
            Fractions = fractions.ToImmutableArray();
        }

        public DatabaseVersion(ImmutableArray<int> fractions)
        {
            Fractions = fractions;
        }

        public ImmutableArray<int> Fractions { get; }
        
        public static DatabaseVersion Parse(string versionString)
        {
            var stringFractions = versionString.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> fractions = new List<int>();
            foreach (string stringFraction in stringFractions)
            {
                try
                {
                    fractions.Add(int.Parse(stringFraction));
                }
                catch (FormatException e)
                {
                    throw new FormatException($"Invalid DatabaseVersion string '{versionString}'");
                }
            }

            return new DatabaseVersion(fractions.ToImmutableArray());
        }

        public int CompareTo(DatabaseVersion other)
        {
            for (int i = 0; i < Math.Max(Fractions.Length, other.Fractions.Length); i++)
            {
                int thisPart = i < Fractions.Length ? Fractions[i] : 0;
                int otherPart = i < other.Fractions.Length ? other.Fractions[i] : 0;
                int diff = thisPart - otherPart;
                if (diff != 0)
                {
                    return diff;
                }
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Join(".", Fractions);
        }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Fractions), Fractions.AsValueObject());
        }
    }
}