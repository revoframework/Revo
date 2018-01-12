using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Security;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Events.Metadata
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
