using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Metadata
{
    public interface IEventMetadataProvider
    {
        Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage);
    }
}
