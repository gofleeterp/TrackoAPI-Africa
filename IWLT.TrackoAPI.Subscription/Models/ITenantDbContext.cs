using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public interface ITenantDbContext
    {
        DbSet<WebApiUsage> ApiLog { get; set; }
        DbSet<Application> Applications { get; set; }
        DbSet<DatabaseBackupLog> BackupLogs { get; set; }
        DbSet<JobTrack> Jobs { get; set; }
        DbSet<TenantMaster> Tenants { get; set; }
        DbSet<EventType> EventTypes { get; set; }
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken());
    }
}