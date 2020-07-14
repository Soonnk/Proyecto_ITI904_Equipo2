using Microsoft.Owin;
using Owin;


namespace Proyecto_ITI904_Equipo2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}