using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class TenantDbContext : DbContext, ITenantDbContext
    {
        //public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
        //{
            
        //    //InteractiveViews.SetViewCacheFactory(this, new FileViewCacheFactory(AppDomain.CurrentDomain.BaseDirectory + "\\TenantDbContext.views.xml"));
        //   //Database.SetInitializer(new MigrateDatabaseToLatestVersion<TenantDbContext,Configuration>());
           
        //}

        public TenantDbContext(DbContextOptions options) : base(options)
        {
        }

        #region Overrides of DbContext

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscriber>(builder =>
            {
                builder.HasMany(x => x.SubscribedEvents);
                builder.HasIndex(x => x.Name).IsUnique();
            });
            modelBuilder.Entity<EventType>(builder =>
            {
                builder.HasMany(x => x.Subscribers);
                builder.HasIndex(x => x.Name).IsUnique();
            });
            modelBuilder.Entity<JobTrack>().Ignore(x => x.EventData);
            modelBuilder.Entity<TenantApplicationMapping>(builder =>
                {
                    builder.HasOne(x=>x.fk_Tenant).WithMany(x => x.Applications).HasForeignKey(x => x.TenantId)
                        .IsRequired();
                    builder.HasOne(x=>x.fk_Application).WithMany(x => x.Tenants).HasForeignKey(x => x.ApplicationId)
                        .IsRequired();
                });
            modelBuilder.Entity<Application>(builder =>
            {
                builder.HasIndex(x => x.ApplicationName).IsUnique();
            });
            modelBuilder.Entity<TenantMaster>(builder =>
            {
                builder.HasIndex(x => x.AccessCode).IsUnique();
                builder.HasIndex(x => x.ClientKey).IsUnique();
                builder.HasIndex(x => x.PANNo).IsUnique();
                builder.HasIndex(x => x.Name).IsUnique();
                builder.HasIndex(x => x.ShortName).IsUnique();
            });
            modelBuilder.Entity<JobTrack>(builder =>
                {
                    builder.HasOne(x => x.fk_EventType).WithMany().HasForeignKey(x => x.EventCode);
                    builder.HasOne(x => x.fk_Tenant).WithMany().HasForeignKey(x => x.TenantId);
                    builder.HasOne(x => x.fk_Sender).WithMany().HasForeignKey(x => x.SenderId);
                    builder.Ignore(x => x.EventData);
                });
           
            base.OnModelCreating(modelBuilder);
        }


        #endregion

        public DbSet<TenantMaster> Tenants { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<WebApiUsage> ApiLog { get; set; }
        public DbSet<DatabaseBackupLog> BackupLogs { get; set; }
        public DbSet<JobTrack> Jobs { get; set; }
        public DbSet<Subscriber> Integrations { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        

        public DbSet<TenantApplicationMapping> TenantApplications { get; set; }
    }
}
