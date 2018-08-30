using System.Threading.Tasks;

namespace Revo.DataAccess.Sequences
{
    public interface ITenantContextSequence<TTenantKey, TContextKey, TValue>
    {
        TValue NextValue(TTenantKey tenantKey, TContextKey contextKey);
        Task<TValue> NextValueAsync(TTenantKey tenantKey, TContextKey contextKey);
    }
}
