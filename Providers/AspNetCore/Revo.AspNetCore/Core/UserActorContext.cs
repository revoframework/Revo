using Revo.Core.Core;
using Revo.Core.Security;

namespace Revo.AspNetCore.Core
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
