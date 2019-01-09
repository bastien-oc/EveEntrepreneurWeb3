using System;
using EveEntrepreneurWebPersistency3.Logic;
using Hangfire;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartupAttribute(typeof(EveEntrepreneurWebPersistency3.Startup))]

namespace EveEntrepreneurWebPersistency3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            GlobalConfiguration.Configuration
                               .UseSqlServerStorage("Data Source=HADES;Initial Catalog=EvePersistency;Integrated Security=True");

            app.UseHangfireServer();
            app.UseHangfireDashboard();
        }
    }
}
