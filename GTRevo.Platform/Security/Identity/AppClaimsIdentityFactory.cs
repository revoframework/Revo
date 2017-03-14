using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security.Identity
{
    public class AppClaimsIdentityFactory : ClaimsIdentityFactory<IUser, Guid>
    {
        internal const string IdentityProviderClaimType =
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

        internal const string DefaultIdentityProviderClaimValue = "ASP.NET Identity";

        public override async Task<ClaimsIdentity> CreateAsync(UserManager<IUser, Guid> manager, IUser user,
            string authenticationType)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            var id = new PermissionClaimsIdentity(authenticationType, UserNameClaimType, RoleClaimType);
            id.AddClaim(new Claim(UserIdClaimType, ConvertIdToString(user.Id), ClaimValueTypes.String));
            id.AddClaim(new Claim(UserNameClaimType, user.UserName, ClaimValueTypes.String));
            id.AddClaim(new Claim(IdentityProviderClaimType, DefaultIdentityProviderClaimValue, ClaimValueTypes.String));
            if (manager.SupportsUserSecurityStamp)
            {
                id.AddClaim(new Claim(SecurityStampClaimType,
                    await manager.GetSecurityStampAsync(user.Id)));
            }
            if (manager.SupportsUserRole)
            {
                IList<string> roles = await manager.GetRolesAsync(user.Id);
                foreach (string roleName in roles)
                {
                    id.AddClaim(new Claim(RoleClaimType, roleName, ClaimValueTypes.String));
                }
            }
            if (manager.SupportsUserClaim)
            {
                id.AddClaims(await manager.GetClaimsAsync(user.Id));
            }
            return id;
        }
    }
}
