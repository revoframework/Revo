using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Revo.Domain.Entities;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess
{
    public abstract class StaticClassifierDatabaseInitializer<T> : IDatabaseInitializer
        where T : class, IAggregateRoot
    {
        public virtual IEnumerable<T> All
        {
            get
            {
                return this.GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(x => (x.IsLiteral || x.IsInitOnly)
                                && x.FieldType == typeof(T)).Select(x => (T)x.GetValue(null));
            }
        }

        public async Task InitializeAsync(IRepository repository)
        {
            var known = All;
            var knownIds = known.Select(x => x.Id).ToArray();
            var existing = await repository.FindManyAsync<T>(knownIds);

            foreach (T entity in known.Where(x => !existing.Any(y => y.Id == x.Id)))
            {
                repository.Add(entity);
            }
        }
    }
}
