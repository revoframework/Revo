using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Revo.EF6.DataAccess.InMemory
{
    public class LocalDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public LocalDbAsyncEnumerable(IEnumerable<T> enumerable) 
            : base(enumerable) 
        { }

        public LocalDbAsyncEnumerable(Expression expression) 
            : base(expression) 
        { }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new LocalDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new LocalDbAsyncQueryProvider<T>(this); }
        }
    }

}
