using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Security;

namespace Revo.Testing.Security
{
    public class FakeUserContext : IUserContext
    {
        public IUser FakeUser { get; set; }
        public List<Permission> FakePermissions { get; set; }

        public bool IsAuthenticated => FakeUser != null;
        public Guid? UserId => FakeUser?.Id;

        public Task<IUser> GetUserAsync()
        {
            return Task.FromResult(FakeUser);
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>(FakePermissions ?? new List<Permission>());
        }

        public void SetFakeUser()
        {
            FakeUser = Substitute.For<IUser>();
            FakeUser.Id.Returns(Guid.NewGuid());
            FakeUser.UserName.Returns(Guid.NewGuid().ToString());
        }
    }
}
