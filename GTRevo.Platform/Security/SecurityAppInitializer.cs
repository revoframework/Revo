using System;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Security.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Ninject;
using Owin;

namespace GTRevo.Platform.Security
{
    public class SecurityAppInitializer : IOwinConfigurator
    {
        private readonly StandardKernel kernel;

        public SecurityAppInitializer(StandardKernel kernel)
        {
            this.kernel = kernel;
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }
        public static string PublicClientId { get; private set; }

        public void ConfigureApp(IAppBuilder app)
        {
            kernel.Bind<IDataProtectionProvider>().ToConstant(app.GetDataProtectionProvider()).InSingletonScope();

            // Configure the db context, user manager and signin manager to use a single instance per request
            //app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<AppUserManager>((options, owinContext)
                => NinjectWebLoader.Bootstrapper.Kernel.Get<AppUserManager>());
            //app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/login"),
                CookieHttpOnly = true,
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<AppUserManager, IUser, Guid>(
                        validateInterval: TimeSpan.FromMinutes(15),
                        regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager, Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie),
                        getUserIdCallback: claimsIdentity => Guid.Parse(claimsIdentity.GetUserId<string>())),

                    OnApplyRedirect = ctx =>
                    {
                        ctx.Response.Redirect(ctx.RedirectUri);
                    }
                },
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(60)
            });

            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/api/token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                //AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
#if DEBUG
                AllowInsecureHttp = true
#endif
            };

            app.UseOAuthBearerTokens(OAuthOptions);

            /*
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            */

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}
