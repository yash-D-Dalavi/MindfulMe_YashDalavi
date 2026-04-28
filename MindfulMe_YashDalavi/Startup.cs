using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MindfulMe_YashDalavi.Startup))]
namespace MindfulMe_YashDalavi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
