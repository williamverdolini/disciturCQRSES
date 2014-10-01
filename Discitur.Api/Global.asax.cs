using Castle.Windsor;
using Discitur.Api.Injection.Installers;
using Discitur.Api.Injection.WebAPI;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;

namespace Discitur.Api
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly IWindsorContainer container;

        public WebApiApplication()
        {
            this.container = new WindsorContainer()
                .Install(
                        new CommandStackInstaller(),
                        new QueryStackInstaller(),
                        new EventStoreInstaller(),
                        new ControllersInstaller()
                        );
        }

        public override void Dispose()
        {
            this.container.Dispose();
            base.Dispose();
        }


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Database.SetInitializer<DisciturContext>(null);

            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(this.container));
        }
    }
}