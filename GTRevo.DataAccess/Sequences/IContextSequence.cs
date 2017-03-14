using System.Threading.Tasks;

namespace GTRevo.DataAccess.Sequences
{
    public interface IContextSequence<TContextKey, TValue>
    {
        TValue NextValue(TContextKey contextKey);
        Task<TValue> NextValueAsync(TContextKey contextKey);
    }
}
