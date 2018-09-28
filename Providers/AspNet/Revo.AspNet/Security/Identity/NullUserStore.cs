using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Revo.AspNet.Security.Identity
{
    public class NullUserStore : IUserStore<IIdentityUser, Guid>
    {
        private const string NotImplementedMessage = "Cannot query users with default NullUserStore. If you want to use authentication or authorization features, you have to register your own non-fake implementation.";

        public IQueryable<IIdentityUser> Users => throw new NotImplementedException(NotImplementedMessage);

        public Task CreateAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task UpdateAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task DeleteAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IIdentityUser> FindByIdAsync(Guid userId)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IIdentityUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }
        
        public void Dispose()
        {
        }
    }
}