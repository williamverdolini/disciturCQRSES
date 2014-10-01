using Microsoft.Owin.Security.OAuth;
using System.Linq;
using System.Web.Http;

namespace Discitur.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.

            //While the MVC templates use a cookie based authentication mechanism, 
            //the new SPA templates prefer to use a token based authentication model 
            //explicitly passed via the Authorization HTTP header (which is better since it avoids CSRF attacks). 
            //This means that the default authentication from the host must be ignored 
            //since the authentication will be performed against something else other than a cookie. 
            //Web API 2 added a feature to ignore the host level authentication called SuppressDefaultHostAuthentication. 
            //This is an extension method on the HttpConfiguration that adds a message handler. 
            //The purpose of this message handler is to simply (and explicitly) assign an anonymous principal 
            //to the RequestContext’s Principal property. 
            //This way if cookie middleware does process an incoming cookie, 
            //by the time the call arrives at Web API the caller will be treated as anonymous
            // http://brockallen.com/2013/10/27/host-authentication-and-web-api-with-owin-and-active-vs-passive-authentication-middleware/

            config.SuppressDefaultHostAuthentication();

            //in Web API 2 a new filter was added: AuthenticationFilter. 
            //This is a dedicated stage in the Web API pipeline for inspecting and authenticating the HTTP request. 
            //You can write your own, or you can use the new built-in HostAuthenticationFilter
            //the HostAuthenticationFilter accepts a constructor parameter indicating which type of authentication 
            //we will use from the host. In the SPA templates they use one called “Bearer” (which is different than cookie).
            //Web API is deferring back out to the host is because of the move to OWIN authentication middleware. 
            //OWIN authentication middleware is providing a generic and host independent framework for authentication
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            //config.EnableSystemDiagnosticsTracing();

            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(
                config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml"));
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(
                config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "text/html"));
            
            // Use camel case for JSON data.
            //config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
