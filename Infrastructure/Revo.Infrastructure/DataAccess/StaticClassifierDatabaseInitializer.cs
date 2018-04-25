using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ninject;
using Revo.Domain.Entities;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.DataAccess
{
    public abstract class StaticClassifierDatabaseInitializer<T> : IDatabaseInitializer
        where T : class, IAggregateRoot, IQueryableEntity
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
            IList<T> existing = await repository.FindAllAsync<T>();
            IEnumerable<T> known = this.All;

            foreach (T entity in /*known.Except(existing)*/
                known.Where(x => !existing.Any(y => y.Id == x.Id)))
            {
                repository.Add(entity);
            }
        }
    }
}
