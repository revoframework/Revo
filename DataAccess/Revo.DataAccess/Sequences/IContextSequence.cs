using System.Threading.Tasks;

namespace Revo.DataAccess.Sequences
{
    public interface IContextSequence<TContextKey, TValue>
    {
        TValue NextValue(TContextKey contextKey);
        Task<TValue> NextValueAsync(TContextKey contextKey);
    }
}
