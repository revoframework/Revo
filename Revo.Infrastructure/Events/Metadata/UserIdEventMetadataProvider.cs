using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Security;

namespace Revo.Infrastructure.Events.Metadata
{
    public class UserIdEventMetadataProvider(IUserContext userContext) : IEventMetadataProvider
    {
        public Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage)
        {
            return Task.FromResult(new[]
            {
                (BasicEventMetadataNames.UserId, userContext.UserId?.ToString())
            });
        }
    }
}
