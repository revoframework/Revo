using GTRevo.Core.Core;
using GTRevo.Core.Security;
using GTRevo.Platform.Security;

namespace GTRevo.Platform.Core
{
    public class UserActorContext : IActorContext
    {
        private readonly IUserContext userContext;

        public UserActorContext(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public string CurrentActorName
        {
            get
            {
                return $"User({userContext.UserId?.ToString() ?? "anonymous"})";
            }
        }
    }
}
