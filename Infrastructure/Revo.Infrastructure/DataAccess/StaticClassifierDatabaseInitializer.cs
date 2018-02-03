using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [Inject] // exceptionally not using constructor injection to make usage easier
        public IRepository Repository { get; set; }

        public void Initialize()
        {
            if (Repository == null)
            {
                throw new InvalidOperationException($"Cannot initialize {this.GetType().FullName} without first setting Repository");
            }

            List<T> existing = Repository.FindAll<T>().ToList();
            IEnumerable<T> known = this.All;

            foreach (T entity in /*known.Except(existing)*/
                known.Where(x => !existing.Any(y => y.Id == x.Id)))
            {
                Repository.Add(entity);
            }

            Repository.SaveChanges();
        }
    }
}
