using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Security;

namespace Revo.Infrastructure.Events.Metadata
{
    public class UserIdEventMetadataProvider : IEventMetadataProvider
    {
        private readonly IUserContext userContext;

        public UserIdEventMetadataProvider(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public Task<(string key, string value)[]> GetMetadataAsync(IEventMessage eventMessage)
        {
            return Task.FromResult(new[]
            {
                (BasicEventMetadataNames.UserId, userContext.UserId?.ToString())
            });
        }
    }
}
