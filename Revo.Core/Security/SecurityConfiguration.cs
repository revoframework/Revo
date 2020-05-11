namespace Revo.Core.Security
{
    public class SecurityConfiguration
    {
        /// <summary>Determines whether to use the default Null security module.</summary>
        /// <remarks>Null security module registers the NullClaimsPrincipalUserResolver that always resolves
        /// the current user to be null and NullUserPermissionResolver that always resolves to no permissions.
        /// Set this to false if you are implementing and registering any of these resolvers on your own.</remarks>
        public bool UseNullSecurityModule { get; set; } = true;
    }
}