using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Tenant.Models.CRM;
using Tenant.TenantMigrations;
using TrackoApi.Core.Helpers;

namespace Tenant.Models
{
    public class CoreSettingDb: DbContext
    {
        public CoreSettingDb():this("name=HostCoreConnection")
        {

        }
        public CoreSettingDb(string connection):base(connection)
        {
            if (Helper.HostedOnPremise)
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<CoreSettingDb, Configuration<CoreSettingDb>>());
            }
        }
        public DbSet<NotificationPurchase> NotificationPurchaseLog { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<WebApiUsage> ApiLog { get; set; }
        public DbSet<JsonGlobalLog> JsonLog { get; set; }
       
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var mdl = modelBuilder.Entity<TenantReportRequestPool>();
            mdl.HasRequired(x => x.fk_Proc).WithMany().HasForeignKey(x => x.ProcId).WillCascadeOnDelete(false);
            mdl.Ignore(x => x.Debug);
            base.OnModelCreating(modelBuilder);
        }
        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            var result = base.ValidateEntity(entityEntry, items);
            switch (entityEntry.Entity)
            {
                case NotificationLog log:
                    var purchase = Set<NotificationPurchase>().OrderBy(x => x.PurchaseTime).FirstOrDefault(x => (!x.ExpiryTime.HasValue || x.ExpiryTime.Value >= DateTime.Now) && x.TenantId == log.TenantId && x.NotificationType == log.NotificationType && (x.Notifications.Where(y => y.IsSent && y.Id != log.Id).Sum(y => (int?)y.NoOfNotification) ?? 0) < x.NoOfNotification);
                    log.fk_Purchase = purchase ?? throw new BusinessException(ErrorCode.EventFailed, $"Insufficient {log.NotificationType.ToString()} balance");
                    log.PurchaseId = purchase.Id;
                    break;
                default:
                    break;
            }
            return result;
        }
        public async Task<int> ExecuteProcedureAsync(string sql, params object[] parameters)
        {
            var existingconnection = this.Database.CurrentTransaction != null || this.Database.Connection.State == ConnectionState.Open;
            var connection = this.Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? this.Database.Connection;

            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                int count = 0;
                try
                {
                    if (!existingconnection)
                    {
                        await connection.OpenAsync();
                    }
                    else
                    {
                        command.Transaction = this.Database.CurrentTransaction?.UnderlyingTransaction;
                    }
                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }
                    count = command.ExecuteNonQuery();
                }
                finally
                {
                    if (!existingconnection)
                        connection.Close();
                    command.Parameters.Clear();
                }
                return count;
            }
        }
        public int ExecuteProcedure(string sql, params object[] parameters)
        {
            var existingconnection = this.Database.CurrentTransaction != null || this.Database.Connection.State == ConnectionState.Open;
            var connection = this.Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? this.Database.Connection;

            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                int count = 0;
                try
                {
                    if (!existingconnection)
                    {
                        connection.Open();
                    }
                    else
                    {
                        command.Transaction = this.Database.CurrentTransaction?.UnderlyingTransaction;
                    }
                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }
                    count = command.ExecuteNonQuery();
                }
                finally
                {
                    if (!existingconnection)
                        connection.Close();
                    command.Parameters.Clear();
                }

                return count;
            }
        }
        public async Task<DataTable> SqlQueryAsync(string sql, params object[] parameters)
        {
            var existingconnection = this.Database.CurrentTransaction != null || this.Database.Connection.State == ConnectionState.Open;
            var connection = this.Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? this.Database.Connection;
            var dt = new DataTable();
            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                try
                {
                    if (!existingconnection)
                    {
                        await connection.OpenAsync();
                    }
                    else
                    {
                        command.Transaction = this.Database.CurrentTransaction?.UnderlyingTransaction;
                    }

                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
                finally
                {
                    if (!existingconnection)
                        connection.Close();
                    command.Parameters.Clear();
                }
            }
            return dt;
        }
        public async Task<DataSet> SqlQueryDataSetAsync(string sql, IDictionary<string, string> tableNameMapping = null, params object[] parameters)
        {
            var database = this.Database;
            var dt = new DataSet();
            using (DbCommand command = database.Connection.CreateCommand())
            {
                try
                {
                    await database.Connection.OpenAsync();
                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }
                    var factory = DbProviderFactories.GetFactory(database.Connection);
                    var adapter = factory.CreateDataAdapter();
                    if (adapter == null) throw new BusinessException(ErrorCode.GLB107, "Invalide Database Type");
                    if (tableNameMapping != null && tableNameMapping.Count > 0)
                    {
                        var mapping = tableNameMapping.Select(x => new DataTableMapping(x.Key, x.Value)).ToArray();
                        adapter.TableMappings.AddRange(mapping);
                    }
                    adapter.SelectCommand = command;
                    adapter.Fill(dt, "Table");
                }
                finally
                {
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return dt;
        }
    }
    public class TenantDbContext : CoreSettingDb, ITenantDbContext
    {
        public TenantDbContext() : base("name=TenantHost")
        {
            //InteractiveViews.SetViewCacheFactory(this, new FileViewCacheFactory(AppDomain.CurrentDomain.BaseDirectory + "\\TenantDbContext.views.xml"));
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TenantDbContext, Configuration<TenantDbContext>>());
        }

        #region Overrides of DbContext

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FuelCompany>().HasKey(x => x.CompanyCode);
            modelBuilder.Entity<JsonGlobalLog>().HasKey(x => new { x.KeyPrefix, x.JsonKey });
            modelBuilder.Entity<StateMaster>().HasKey(x=>x.Id).HasRequired(x=>x.Company).WithMany(x=>x.States).HasForeignKey(x=>x.CompanyCode).WillCascadeOnDelete(false);
            modelBuilder.Entity<StateMaster>().HasMany(x=>x.Towns).WithRequired(x=>x.State).HasForeignKey(x=>x.StateId).WillCascadeOnDelete(false);
            modelBuilder.Entity<IOCPump>().HasKey(x => x.PumpId).HasRequired(x => x.Comany).WithMany(x=>x.Pumps).HasForeignKey(x => x.CompanyCode).WillCascadeOnDelete(false);
            modelBuilder.Entity<IOCPump>().HasRequired(x=> x.State).WithMany(x=>x.Pumps).HasForeignKey(x=> x.StateId).WillCascadeOnDelete(false);
            modelBuilder.Entity<IOCPump>().HasOptional(x=>x.Town).WithMany(x=>x.Pumps).HasForeignKey(x=>x.TownId).WillCascadeOnDelete(false);
            modelBuilder.Entity<RateLog>().HasOptional(x=>x.HPCLTown).WithMany(x=>x.RateLogs).HasForeignKey(x=>x.TownCode).WillCascadeOnDelete(false);
            modelBuilder.Entity<RateLog>().HasOptional(x => x.IocPump).WithMany(x => x.RateLogs).HasForeignKey(x => x.PumpId).WillCascadeOnDelete(false);
            modelBuilder.Entity<HPCLTown>().HasKey(x=>x.Id).HasMany(x=>x.Pumps).WithOptional(x=>x.Town).HasForeignKey(x=>x.TownId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Subscriber>().HasMany(x => x.Events).WithMany(x => x.Subscribers);

            var dps = modelBuilder.Entity<DPS>();
            dps.Ignore(x => x.Data);
            dps.HasRequired(x => x.fk_WorkItem).WithMany().HasForeignKey(x => x.WorkItemId).WillCascadeOnDelete(false);
            dps.HasOptional(x => x.fk_WorkItemLog).WithMany().HasForeignKey(x => x.WorkItemLogId).WillCascadeOnDelete(false);


            var rn=modelBuilder.Entity<ReleaseNote>().Ignore(x => x.Data);
            rn.HasRequired(x => x.fk_Application).WithMany().HasForeignKey(x => x.ApplicationId).WillCascadeOnDelete(false);
            rn.HasRequired(x => x.fk_Tenant).WithMany().HasForeignKey(x => x.TenantId).WillCascadeOnDelete(false);


            modelBuilder.Entity<TenantConstantValue>().Ignore(x => x.Data);
            modelBuilder.Entity<TenantConstantType>().Ignore(x => x.Data);


            var wi=modelBuilder.Entity<WorkItem>().Ignore(x => x.Data);
            wi.HasOptional(x => x.fk_Application).WithMany().HasForeignKey(x => x.ApplicationId).WillCascadeOnDelete(false);
            wi.HasRequired(x => x.fk_Impact).WithMany().HasForeignKey(x => x.ImpactId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_ObjectType).WithMany().HasForeignKey(x => x.ObjectTypeId).WillCascadeOnDelete(false);
            wi.HasRequired(x => x.fk_Priority).WithMany().HasForeignKey(x => x.PriorityId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_Release).WithMany().HasForeignKey(x => x.ReleaseId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_Resolution).WithMany().HasForeignKey(x => x.ResolutionId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_Status).WithMany().HasForeignKey(x => x.StatusId).WillCascadeOnDelete(false);
            wi.HasRequired(x => x.fk_Tenant).WithMany().HasForeignKey(x => x.TenantId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_View).WithMany().HasForeignKey(x => x.ViewId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_WorkItemRef).WithMany().HasForeignKey(x => x.WorkItemRefId).WillCascadeOnDelete(false);
            wi.HasOptional(x => x.fk_WorkType).WithMany().HasForeignKey(x => x.WorkTypeId).WillCascadeOnDelete(false);

            var wic=modelBuilder.Entity<WorkItemComment>().Ignore(x => x.Data);
            wic.HasRequired(x => x.fk_WorkItem).WithMany().HasForeignKey(x => x.WorkItemId).WillCascadeOnDelete(false);
            wic.HasOptional(x => x.fk_CommentRef).WithMany().HasForeignKey(x => x.CommentRefId).WillCascadeOnDelete(false);
            wic.HasOptional(x => x.fk_WorkItemLog).WithMany().HasForeignKey(x => x.WorkItemLogId).WillCascadeOnDelete(false);

            var wil=modelBuilder.Entity<WorkItemLog>().Ignore(x => x.Data);
            wil.HasOptional(x => x.fk_WorkItem).WithMany().HasForeignKey(x => x.WorkItemId).WillCascadeOnDelete(false);
            wil.HasRequired(x => x.fk_Status).WithMany().HasForeignKey(x => x.StatusId).WillCascadeOnDelete(false);

            var wirl=modelBuilder.Entity<WorkItemReferenceLog>().Ignore(x => x.Data);
            wirl.HasRequired(x => x.fk_ParentWorkItem).WithMany().HasForeignKey(x => x.ParentWorkItemId).WillCascadeOnDelete(false);
            wirl.HasRequired(x => x.fk_RefWorkItem).WithMany().HasForeignKey(x => x.RefWorkItemId).WillCascadeOnDelete(false);

            var wism=modelBuilder.Entity<WorkItemStatusMap>().Ignore(x => x.Data);
            wism.HasRequired(x => x.fk_Status).WithMany().HasForeignKey(x => x.StatusId).WillCascadeOnDelete(false);
            wism.HasRequired(x => x.fk_NextStatus).WithMany().HasForeignKey(x => x.NextStatusId).WillCascadeOnDelete(false);

            var wdr=modelBuilder.Entity<WorkDeliveryReport>().Ignore(x => x.Data);
            wdr.HasRequired(x => x.fk_WorkItem).WithMany().HasForeignKey(x => x.WorkItemId).WillCascadeOnDelete(false);
            //modelBuilder.Entity<JobTrack>().Ignore(x => x.EventData).Ignore(x=>x.Events);

            //var mdl = modelBuilder.Entity<TenantReportRequestPool>();
            //mdl.HasRequired(x=>x.fk_Proc).WithMany().HasForeignKey(x=>x.ProcId).WillCascadeOnDelete(false);
            //mdl.Ignore(x => x.Debug);
            base.OnModelCreating(modelBuilder);
        }

        #endregion
        public DbSet<TenantReportRequestPool> ReportRequestPool { get; set; }
        public DbSet<TenantReportProcedure> ReportProcedure { get; set; }
        public DbSet<ThirdPartyToken> ThirdPartyTokens { get; set; }
      
        public DbSet<TenantMaster> Tenants { get; set; }
        public DbSet<Application> Applications { get; set; }
        
        public DbSet<DatabaseBackupLog> BackupLogs { get; set; }
        public DbSet<JobTrack> Jobs { get; set; }
        public static TenantDbContext Create() => new TenantDbContext();

        public DbSet<StateMaster> States { get; set; }
        public DbSet<FuelCompany> FuelCompanies { get; set; }
        public DbSet<IOCPump> Pumps { get; set; }
        public DbSet<RateLog> RateLogs { get; set; }
        public DbSet<HPCLTown> Towns { get; set; }
        public DbSet<TollPlaza> Tolls { get; set; }
        public DbSet<Subscriber> Integrations { get; set; }
        public DbSet<IntegrationEventMaster> IntegrationEvents { get; set; }
        public DbSet<TenantApplicationMapping> TenantApplications { get; set; }
        public DbSet<DPS> DevPerfSheets { get; set; }
        public DbSet<ReleaseNote> ReleaseNotes { get; set; }
        public DbSet<TenantConstantValue> Constants { get; set; }
        public DbSet<TenantConstantType> ConstantTypes { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkItemComment> WorkItemComments { get; set; }
        public DbSet<WorkItemLog> WorkItemLogs { get; set; }
        public DbSet<WorkItemReferenceLog> WorkItemReferenceLogs { get; set; }
        public DbSet<WorkItemStatusMap> WorkItemStatusMapping { get; set; }
        public DbSet<WorkDeliveryReport> WorkDeliveryReports { get; set; }
        
        public bool IsODataBatchContext { get; private set; }

        public DbTransaction ODataBatchBeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            //_objectContext = ((IObjectContextAdapter) _dataContext).ObjectContext;
            if (this.Database.Connection.State != ConnectionState.Open)
            {
                this.Database.Connection.Open();
            }
            //if (_dataContext.Database.CurrentTransaction == null)
            //{
            //    _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
            //}
            IsODataBatchContext = true;
            return this.Database.Connection.BeginTransaction(isolationLevel);
        }
    }
}
