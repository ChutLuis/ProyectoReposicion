using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Reposición.Startup))]
namespace Reposición
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
