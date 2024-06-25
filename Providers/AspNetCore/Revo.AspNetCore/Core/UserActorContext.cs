using Revo.Core.Core;
using Revo.Core.Security;

namespace Revo.AspNetCore.Core
{
    public class UserActorContext(IUserContext userContext) : IActorContext
    {
        public string CurrentActorName
        {
            get
            {
                return $"User({userContext.UserId?.ToString() ?? "anonymous"})";
            }
        }
    }
}
