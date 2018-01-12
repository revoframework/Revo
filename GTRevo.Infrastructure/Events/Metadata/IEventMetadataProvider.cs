using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Metadata
{
    public interface IEventMetadataProvider
    {
        Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage);
    }
}
