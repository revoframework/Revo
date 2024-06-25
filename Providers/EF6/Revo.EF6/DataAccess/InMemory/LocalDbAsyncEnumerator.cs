using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.EF6.DataAccess.InMemory
{
    public class LocalDbAsyncEnumerator<T>(IEnumerator<T> inner) : IDbAsyncEnumerator<T>
    {

        public void Dispose()
        {
            inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(inner.MoveNext());
        }

        public T Current
        {
            get { return inner.Current; }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }
    }
}
