using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace Tenant.TenantMigrations
{

    internal sealed class Configuration<T> : DbMigrationsConfiguration<T> where T: DbContext
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Tenant.Migrations.Configuration";
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(T context)
        {
           //if (context.Tenants.Any()) return;
           // var list = new List<TenantMaster>
           // {
           //     new TenantMaster()
           //     {
           //         ClientKey = "8UwgmpZIO7KwVf88B9MwtwoUsi9wefXmpwmP9Rumwck=",
           //         ConnectionString = "Server=tcp:tjfqdu0vyn.database.windows.net,1433;Database=Tracko;User ID=trackouser@tjfqdu0vyn;Password=Mukesh@1463;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;",
           //         Name = "Sanjay Transport COmpany"
           //     },
           //     new TenantMaster()
           //     {
           //         ClientKey = "8UwgmpZIO7KwVf88B9MwtwoUsi9wefXmpwmP9Rumwck=",
           //         ConnectionString = "Server=tcp:tjfqdu0vyn.database.windows.net,1433;Database=Tracko;User ID=trackouser@tjfqdu0vyn;Password=Mukesh@1463;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;",
           //         Name = "Sanjay Transport COmpany"
           //     }
           // };
           // context.Tenants.AddRange(list);
           // context.SaveChanges();
        }
    }
}
