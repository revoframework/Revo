using System.Threading.Tasks;

namespace Revo.Infrastructure.Events.Async
{
    public interface IEventSourceCatchUp
    {
        Task CatchUpAsync();
    }
}
