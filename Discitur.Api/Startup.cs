using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Discitur.Api.Startup))]

namespace Discitur.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}