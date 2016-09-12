using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Gobot.Startup))]
namespace Gobot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
