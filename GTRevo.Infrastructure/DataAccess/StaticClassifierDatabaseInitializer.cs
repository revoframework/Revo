using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Repositories;
using Ninject;

namespace GTRevo.Infrastructure.DataAccess
{
    public abstract class StaticClassifierDatabaseInitializer<T> : IDatabaseInitializer
        where T : class, IAggregateRoot, IQueryableEntity
    {
        public abstract IEnumerable<T> All { get; }

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
