using Discitur.CommandStack.Worker;
using Discitur.Infrastructure;
using Discitur.QueryStack;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.Worker;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Api.Providers.Authentication
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private readonly Func<UserManager<IdentityUser>> _userManagerFactory;
        // Worker Services
        private readonly IUserQueryWorker QueryWorker;
        private readonly IUserCommandWorker CommandWorker;
        //private DisciturContext db = new DisciturContext();

        public ApplicationOAuthProvider(string publicClientId, Func<UserManager<IdentityUser>> userManagerFactory, IUserQueryWorker queryWorker, IUserCommandWorker commandWorker)
        {
            Contract.Requires<ArgumentNullException>(publicClientId != null, "publicClientId");
            Contract.Requires<ArgumentNullException>(userManagerFactory != null, "userManagerFactory");
            Contract.Requires<System.ArgumentNullException>(queryWorker != null, "queryWorker");
            Contract.Requires<System.ArgumentNullException>(commandWorker != null, "commandWorker");
            _publicClientId = publicClientId;
            _userManagerFactory = userManagerFactory;
            QueryWorker = queryWorker;
            CommandWorker = commandWorker;
        }

        //[Mag14.Controllers.AccountController.JsonResultWebApiFilter]
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (UserManager<IdentityUser> userManager = _userManagerFactory())
            {
                // Sent encrypted password to decrypt with the same algorithm
                IdentityUser user = await userManager.FindAsync(context.UserName, Codec.DecryptStringAES(context.Password));

                if (user == null)
                {
                    context.SetError("discerr03", "The user name or password is incorrect.");
                    //context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

                // TODO: remove bastard instantiation with DI
                UserActivation activation = await (new DisciturContext()).UserActivations.FindAsync(context.UserName);

                if (activation != null)
                {
                    context.SetError("not_activated", "The account is not activated.");
                    return;
                }

                ClaimsIdentity oAuthIdentity = await userManager.CreateIdentityAsync(user, context.Options.AuthenticationType);
                ClaimsIdentity cookiesIdentity = await userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);
                AuthenticationProperties properties = CreateProperties(user.UserName);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(cookiesIdentity);

                try
                {
                    User _user = await QueryWorker.GetUserByUserName(user.UserName);
                    CommandWorker.LogInUser(_user.UserId, DateTime.Now);
                }
                // TODO: gestire l'eccezione di validazione (per loginripetute ello stesso giorno
                catch (Exception) { }
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }


    public static class Codec
    {
        public static string DecryptStringAES(string encryptedWord)
        {
            var keybytes = Encoding.UTF8.GetBytes("7061737323313233");
            var iv = Encoding.UTF8.GetBytes("7061737323313233");

            //c# encrrption
            //var encryptStringToBytes = EncryptStringToBytes("Mattia", keybytes, iv);

            // Decrypt the bytes to a string.
            //var roundtrip = DecryptStringFromBytes(encryptStringToBytes, keybytes, iv);

            //DECRYPT FROM CRIPTOJS
            var encrypted = Convert.FromBase64String(encryptedWord);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return decriptedFromJavascript;
            
            /*
            return string.Format(
                "roundtrip reuslt:{0}{1}Javascript result:{2}",
                roundtrip,
                Environment.NewLine,
                decriptedFromJavascript);
            */
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            byte[] encrypted;
            // Create a RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
    }


}