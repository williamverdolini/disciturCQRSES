using Discitur.Api.Common;
using Discitur.Api.Models;
using Discitur.Api.Providers;
using Discitur.Api.Providers.Authentication;
using Discitur.Api.Result;
using Discitur.CommandStack.ViewModel;
using Discitur.CommandStack.Worker;
using Discitur.Infrastructure;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.Worker;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Security;

namespace Discitur.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";

        // OAuth protocol
        private readonly UserManager<IdentityUser> UserManager;
        private readonly ISecureDataFormat<AuthenticationTicket> AccessTokenFormat;
        // Worker Services
        private readonly IUserQueryWorker QueryWorker;
        private readonly IUserCommandWorker CommandWorker;

        #region Ctors
        public AccountController(IUserQueryWorker queryWorker, IUserCommandWorker commandWorker)
            : this(Startup.UserManagerFactory(), Startup.OAuthOptions.AccessTokenFormat, queryWorker, commandWorker)
        {            
        }

        public AccountController(UserManager<IdentityUser> userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat, IUserQueryWorker queryWorker, IUserCommandWorker commandWorker)
        {
            Contract.Requires<System.ArgumentNullException>(userManager != null, "userManager");
            Contract.Requires<System.ArgumentNullException>(accessTokenFormat != null, "accessTokenFormat");
            Contract.Requires<System.ArgumentNullException>(queryWorker != null, "queryWorker");
            Contract.Requires<System.ArgumentNullException>(commandWorker != null, "commandWorker");
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
            QueryWorker = queryWorker;
            CommandWorker = commandWorker;
        }
        #endregion

        #region Command
        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                CommandWorker.RegisterUser(model);

                // Create new Account 
                IdentityUser user = new IdentityUser
                {
                    UserName = model.UserName
                };

                string decryptedPwd = Codec.DecryptStringAES(model.Password);
                IdentityResult result = UserManager.Create(user, decryptedPwd);
                IHttpActionResult errorResult = GetErrorResult(result);

                if (errorResult != null)
                    return errorResult;

                await MailProvider.GetMailprovider().SendActivationEmail(model.Email, model.UserName, decryptedPwd, model.ActivationKey, Request.Headers.Referrer.AbsoluteUri);

                //TODO: Remove -> Register a user does NOT require to return a User (needs an activation step)
                User discuser = await QueryWorker.GetUserByUserName(model.UserName);
                return Ok(discuser);
            }
            catch (Exception e)
            {
                return BadDisciturRequest(e.Message);
            }
        }


        // POST api/Account/Activate
        [AllowAnonymous]
        [Route("Activate")]
        //public async Task<IHttpActionResult> Activate(UserActivation model)
        public IHttpActionResult Activate(ActivateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //TODO: uniform exception management (FluentValidation). When user is already activated should display a proper message
                CommandWorker.ActivateUser(model);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        
        #endregion

        #region Query
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public async Task<User> GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            string userName = User.Identity.GetUserName();
            bool isAdmin = UserManager.GetRoles(User.Identity.GetUserId()).Contains(Constants.DISCITUR_ADMIN_ROLE);
            User user = await QueryWorker.GetUserByUserName(userName);
            user.IsAdmin = isAdmin;
            return user;
        }
        #endregion

        #region OAuth API
        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), Codec.DecryptStringAES(model.OldPassword), Codec.DecryptStringAES(model.NewPassword));
            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> GetResetPassword(string UserName)
        {
            User discuser;
            IdentityUser _user = await UserManager.FindByNameAsync(UserName);
            if (_user == null)
            {
                ModelState.AddModelError("", "UserName Not Found");
                return BadRequest(ModelState);
            }
            else
            {
                try
                {
                    discuser = await QueryWorker.GetUserByUserName(UserName);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                    return BadRequest(ModelState);
                }
            }

            if (discuser == null)
            {
                ModelState.AddModelError("", "UserName Not Found");
                return BadRequest(ModelState);
            }

            //1° modo:
            /*
            string hashedNewPassword = UserManager.PasswordHasher.HashPassword("prova");
            UserStore<IdentityUser> store = new UserStore<IdentityUser>(db);
            await store.SetPasswordHashAsync(_user, hashedNewPassword);
            await store.UpdateAsync(_user);
            */

            // 2° modo
            //string npwd = Guid.NewGuid().ToString("d").Substring(1, 8);
            string npwd = Membership.GeneratePassword(12, 0);
            UserManager.RemovePassword(_user.Id);
            UserManager.AddPassword(_user.Id, npwd);

            try
            {
                await MailProvider.GetMailprovider().SendForgottenPwdEmail(discuser.Email, npwd);
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return BadRequest(ModelState);
            }

        }

        #endregion

        #region Not yet used API
        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                UserName = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            IdentityUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                ClaimsIdentity oAuthIdentity = await UserManager.CreateIdentityAsync(user,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await UserManager.CreateIdentityAsync(user,
                    CookieAuthenticationDefaults.AuthenticationType);
                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            IdentityUser user = new IdentityUser
            {
                UserName = model.UserName
            };
            user.Logins.Add(new IdentityUserLogin
            {
                LoginProvider = externalLogin.LoginProvider,
                ProviderKey = externalLogin.ProviderKey
            });
            IdentityResult result = await UserManager.CreateAsync(user);
            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserManager.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private IHttpActionResult BadDisciturRequest(string errorCode)
        {
            AddModelError(errorCode);
            return BadRequest(ModelState);
        }

        private void AddModelError(string errorCode)
        {
            ModelState.AddModelError(Constants.DISCITUR_ERRORS, errorCode);
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        private async Task SignInAsync(RegisterBindingModel model, bool isPersistent)
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            // Since ASP.NET Identity and OWIN Cookie Authentication are claims-based system, 
            // the framework requires the app to generate a ClaimsIdentity for the user
            // ClaimsIdentity has information about all the claims for the user, 
            // such as what roles the user belongs to. You can also add more claims for the user at this stage
            IdentityUser user = new IdentityUser
            {
                UserName = model.UserName,
                PasswordHash = model.Password
            };

            ClaimsIdentity oAuthIdentity = await UserManager.CreateIdentityAsync(user,
                OAuthDefaults.AuthenticationType);

            ClaimsIdentity cookieIdentity = await UserManager.CreateIdentityAsync(user,
                CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);

            // signs in the user by using the AuthenticationManager from OWIN 
            // and calling SignIn and passing in the ClaimsIdentity
            Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);

        }

        #endregion

    }
}