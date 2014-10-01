using Discitur.Api.Providers.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Cors;

namespace Discitur.Api
{
    public partial class Startup
    {
        //public const string ExternalCookieAuthenticationType = CookieAuthenticationDefaults.ExternalAuthenticationType;
        public const string ExternalOAuthAuthenticationType = "ExternalToken";

        static Startup()
        {
            PublicClientId = "self";

            UserManagerFactory = () => new UserManager<IdentityUser>(new UserStore<IdentityUser>());


            // Custom Identity Storage Provider
            // ASP.NET Identity has the following high level constructs: Managers and Stores.
            // Managers are high level classes which the application developer uses to do any operation in the ASP.NET Identity system. 
            // The ASP.NET Identity system has the following Managers: UserManager and RolesManager. 
            // UserManager is used to perform any operation on the User such as creating a user or deleting a user. 
            // RoleManager is used to perform operations on Roles.

            // Stores are lower level classes which have the implementation details on how the particular entities 
            // such as User and Roles are stored. Stores are closely coupled with the persistence mechanism. 
            // Eg. Microsoft.ASP.NET.Identity.EntityFramework has UserStore, which is used to store the User type 
            // and its related types using EntityFramework.

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/api/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId, UserManagerFactory),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };

        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static Func<UserManager<IdentityUser>> UserManagerFactory { get; set; }

        //public static CookieAuthenticationOptions CookieOptions { get; private set; }

        //public static IdentityManagerFactory IdentityManagerFactory { get; set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            bool isCorsEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["CORSEnabled"]);
            string corsOrigin = ConfigurationManager.AppSettings["CORSOrigin"];
            if (isCorsEnabled)
            {
                Microsoft.Owin.Cors.CorsOptions corsOptions = new Microsoft.Owin.Cors.CorsOptions();
                corsOptions.PolicyProvider = new ConfigCorsPolicy(corsOrigin);
                app.UseCors(corsOptions);
            }

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = OAuthDefaults.AuthenticationType,
            //    LoginPath = new PathString("/Account/Login")
            //});
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }

    public class ConfigCorsPolicy : ICorsPolicyProvider
    {
        private CorsPolicy _policy;

        public ConfigCorsPolicy(string origin)
        {
            // Create a CORS policy.
            _policy = new CorsPolicy
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true
            };
            // Add allowed origins.
            string [] origins = origin.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach(string o in origins)
                _policy.Origins.Add(o);
            //_policy.Origins.Add(origin);
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {
            return Task.FromResult(_policy);
        }
    }



}