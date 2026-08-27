using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

using Tenant.Models.CRM;

namespace Tenant.Models
{
    public interface ITenantDbContext: IDisposable
    {
        DbSet<WebApiUsage> ApiLog { get; set; }
        DbSet<ThirdPartyToken> ThirdPartyTokens { get; set; }
        DbSet<Application> Applications { get; set; }
        DbSet<DatabaseBackupLog> BackupLogs { get; set; }
        DbSet<FuelCompany> FuelCompanies { get; set; }
        DbSet<JobTrack> Jobs { get; set; }
        DbSet<IOCPump> Pumps { get; set; }
        DbSet<RateLog> RateLogs { get; set; }
        DbSet<StateMaster> States { get; set; }
        DbSet<TenantMaster> Tenants { get; set; }
        DbSet<HPCLTown> Towns { get; set; }
        DbSet<TollPlaza> Tolls { get; set; }
        DbSet<IntegrationEventMaster> IntegrationEvents { get; set; }
        DbSet<JsonGlobalLog> JsonLog { get; set; }
        DbSet<DPS> DevPerfSheets { get; set; }
        DbSet<ReleaseNote> ReleaseNotes { get; set; }
        DbSet<TenantConstantValue> Constants { get; set; }
        DbSet<TenantConstantType> ConstantTypes { get; set; }
        DbSet<WorkItem> WorkItems { get; set; }
        DbSet<WorkItemComment> WorkItemComments { get; set; }
        DbSet<WorkItemLog> WorkItemLogs { get; set; }
        DbSet<WorkItemReferenceLog> WorkItemReferenceLogs { get; set; }
        DbSet<WorkItemStatusMap> WorkItemStatusMapping { get; set; }
        DbSet<WorkDeliveryReport> WorkDeliveryReports { get; set; }
        DbSet<TenantReportRequestPool> ReportRequestPool { get; set; }
        DbSet<TenantReportProcedure> ReportProcedure { get; set; }
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        int SaveChanges();
        DbEntityEntry Entry(object entity);
        DbSet<T> Set<T>() where T : class;
        Database Database { get; }
        DbChangeTracker ChangeTracker { get; }
        DbContextConfiguration Configuration { get; }
        int ExecuteProcedure(string sql, params object[] parameters);
        Task<int> ExecuteProcedureAsync(string sql, params object[] parameters);
        Task<DataTable> SqlQueryAsync(string sql, params object[] parameters);
        Task<DataSet> SqlQueryDataSetAsync(string sql, IDictionary<string, string> tableNameMapping = null, params object[] parameters);
        DbTransaction ODataBatchBeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool IsODataBatchContext { get; }
    }
}