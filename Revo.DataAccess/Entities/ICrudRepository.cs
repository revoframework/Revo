using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.Entities
{
    public interface ICrudRepository : IReadRepository
    {
        void Attach<T>(T entity) where T : class;
        void AttachRange<T>(IEnumerable<T> entities) where T : class;

        void Add<T>(T entity) where T : class;
        void AddRange<T>(IEnumerable<T> entities) where T : class;

        void Remove<T>(T entity) where T : class;

        /// <summary>
        /// Saves the repository changes. Not needed when using unit of work.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Asynchronously save the repository changes. Not needed when using unit of work.
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
