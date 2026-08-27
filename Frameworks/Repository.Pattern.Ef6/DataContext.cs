using AutoMapper;
using CodeFirstStoreFunctions;
using EntityFramework.Caching;
using EntityFramework.Extensions;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Repository.Pattern.DataContext;
using Repository.Pattern.Ef6.Conventions;
using Repository.Pattern.Ef6.Extentions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoAPI.Code.Logics;
using TrackoAPI.Code.Logics.AMS;
using TrackoAPI.Code.Logics.BMS;
using TrackoAPI.Code.Logics.FMS;
using TrackoAPI.Code.Logics.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.AMS;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.vw.ts;

using Rule = TrackoApi.Models.Base.Rule;

namespace Repository.Pattern.Ef6
{
    /// <summary>
    /// Class DataContext.
    /// </summary>
    /// <seealso cref="ApiUserClaim" />
    [DbConfigurationType(typeof(ApiDbConfiguration))]
    public class DataContext : IdentityDbContext<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim>, IDataContextAsync
    {
        #region Private Fields

        private readonly IGlobalStore _globalStore;
        private readonly Guid _instanceId;
        public bool Disposed { get; private set; }
        private bool? RulesAreOn = null;
        private bool? NotificationRulesAreOn = null;

        #endregion Private Fields

        private static IMapper _configArchive2Log = null;

        private static IMapper _configLog2Archive = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataContext"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">The name or connection string.</param>
        public DataContext(IGlobalStore globalStore, string nameOrConnectionString) : base(nameOrConnectionString)
        {
            //DbConfiguration.LoadConfiguration(typeof(ApiDbConfiguration));

            SessionId = PopulateSessionId();
            TenantId = PopulateTenantId();
            _instanceId = Guid.NewGuid();
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            RequireUniqueEmail = false;
            _globalStore = globalStore;
        }
        
        public DbSet<AccountParentChild> AccountGroupChildren { get; set; }

        /// <summary>
        /// Gets or sets the API configurations.
        /// </summary>
        /// <value>The API configurations.</value>
        public DbSet<ApiConfiguration> ApiConfigurations { get; set; }

        public DbSet<vw_CNStockLog> CNStockLogsView { get; set; }

        public DbSet<vw_CNStockMMLog> CNStockMMLogsView { get; set; }
        public DbSet<FaultVersionLog> FaultVersions { get; set; }
        public DbSet<ConversationGroup> ConversationGroups { get; set; }

        /// <summary>
        /// Gets or sets the financial years.
        /// </summary>
        /// <value>The financial years.</value>
        public DbSet<FinancialYear> FinancialYears { get; set; }

        /// <summary>
        /// Gets the instance identifier.
        /// </summary>
        /// <value>The instance identifier.</value>
        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        public DbSet<PermissionSet> Permissions { get; set; }

        public long SessionId { get; private set; }
        public string TenantId { get; private set; }

        public DbSet<UserConnection> UserConnections { get; set; }

        public DbSet<VDRBalance> VDRBalances { get; set; }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            var query = this.Set<TEntity>().Where(expression);
            return Delete<TEntity>(ToObjectQuery(query));
        }

        public int Delete<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            return Delete<TEntity>(ToObjectQuery(query));
        }

        public int Delete<TEntity>(ObjectQuery<TEntity> query) where TEntity : class
        {
            var context = query.Context;
            //Get EntityFramework API Mapping information
            var mapping = GetMapping(context, typeof(TEntity));
            //Given the query, generate a Select statement which selects the Primary Key columns based on the IQueryable<TEntity> passed in
            var innerSelect = GetSelectSql(query, mapping.KeyMembers);
            //build a DELETE statement in the form of :
            //DELETE <Table> FROM <Table> as j0 INNER JOIN (<internalSelect>) as j1 ON <keys>
            var sqlBuilder = new StringBuilder(innerSelect.Sql.Length * 2);
            sqlBuilder.AppendFormat("DELETE {0}\n", mapping.TableName);
            sqlBuilder.AppendFormat("FROM {0} as j0 INNER JOIN (\n", mapping.TableName);
            sqlBuilder.AppendLine(innerSelect.Sql);
            sqlBuilder.Append(") as j1 on (");
            sqlBuilder.Append(string.Join(" AND ", mapping.KeyMembers.Select(x => string.Format("j0.{0} = j1.{0}", x.Name))));
            sqlBuilder.AppendLine(")");
            //Execute the compiled DELETE command with parameters from the generated Select statement
            return context.ExecuteStoreCommand(sqlBuilder.ToString(), innerSelect.Parameters.ToArray<object>());
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
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
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

        public T GetApiClientConfig<T>(string key) where T : struct
        {
            var value = this.Set<ClientConfiguration>().Where(x => x.Id == key).FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)));
            return Utilities.To<T>(value?.ConfigValue);
        }

        public T GetApiConfig<T>(string key) where T : struct
        {            
            var value = this.ApiConfigurations.Where(x => x.Key == key).FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)));
            if (value==null&& !this.ApiConfigurations.Any(x=>x.Key==key))
            {
                var defaultValue = (object)$"{default(T)}"??DBNull.Value;
                this.Database.ExecuteSqlCommand("INSERT INTO ApiConfigurations(Id,[Value],IsReserved)VALUES(@p0,@p1,0)",key, $"{default(T)}");
            }
            return Utilities.To<T>(value?.Value);
        }
        public T GetApiClientConfig<T>(string key, T defaultValue) where T : struct
        {
            var value = this.Set<ClientConfiguration>().Where(x => x.Id == key).FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)));
            return Utilities.To<T>(value?.ConfigValue, defaultValue);
        }

        public T GetApiConfig<T>(string key, T defaultValue) where T : struct
        {
            var value = this.ApiConfigurations.Where(x => x.Key == key).FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)));
            if (value == null && !this.ApiConfigurations.Any(x => x.Key == key))
            {
                this.Database.ExecuteSqlCommand("INSERT INTO ApiConfigurations(Id,[Value],IsReserved)VALUES(@p0,@p1,0)", key, $"{defaultValue}");
            }
            return Utilities.To<T>(value?.Value, defaultValue);
        }

        public string GetApiConfig(string key)
        {
            var value = this.ApiConfigurations.Where(x => x.Key == key).FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)));
            return value?.Value;
        }

        public DataTable GetDataTableByProcedure(string sql, params object[] parameters)
        {
            var existingconnection = Database.CurrentTransaction != null || Database.Connection.State == ConnectionState.Open;
            var connection = Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? Database.Connection;
            var dt = new DataTable();
            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                try
                {
                    if (!existingconnection)
                    {
                        connection.Open();
                    }
                    else
                    {
                        command.Transaction = Database.CurrentTransaction?.UnderlyingTransaction;
                    }

                    command.CommandText = sql.Replace(" ", "").Split('@')[0]; ;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
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

        public long GetDTSStatusIdByDateId(long dateId)
        {
            try
            {
                return this.Database.SqlQuery<long?>($"SELECT TOP 1 Id FROM [dbo].[mDTSStatus] WHERE DateId={dateId}")
                    .FirstOrDefault()
                    .GetValueOrDefault(0);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <exception cref="System.Data.Entity.Infrastructure.DbUpdateException">
        ///     An error occurred sending updates to the database.</exception>
        /// <exception cref="System.Data.Entity.Validation.DbEntityValidationException">
        ///     The save was aborted because validation of entity property values failed.</exception>
        /// <exception cref="System.NotSupportedException">
        ///     An attempt was made to use unsupported behavior such as executing multiple
        ///     asynchronous commands concurrently on the same context instance.</exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     The context or connection have been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Some error occurred attempting to process entities in the context either
        ///     before or after sending commands to the database.</exception>
        /// <seealso cref="DbContext.SaveChanges"/>
        /// <returns>The number of objects written to the underlying database.</returns>
        /// <exception cref="BusinessException">Validation Failed.</exception>
        public override int SaveChanges()
        {
            try
            {
                SyncObjectsStatePreCommit();                
                var changes = base.SaveChanges();
                SyncObjectsStatePostCommit();
                return changes;
            }
            catch (DbEntityValidationException ex)
            {
                throw new BusinessException(ErrorCode.GLB106, ex.EntityValidationErrors);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new BusinessException(ErrorCode.GLB104, ex.GetBaseException().Message);
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException(ex);
            }
            catch (DbException ex)
            {
                throw;
            }
        }

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <exception cref="System.Data.Entity.Infrastructure.DbUpdateException">
        ///     An error occurred sending updates to the database.</exception>
        /// <exception cref="System.Data.Entity.Infrastructure.DbUpdateConcurrencyException">
        ///     A database command did not affect the expected number of rows. This usually
        ///     indicates an optimistic concurrency violation; that is, a row has been changed
        ///     in the database since it was queried.</exception>
        /// <exception cref="System.Data.Entity.Validation.DbEntityValidationException">
        ///     The save was aborted because validation of entity property values failed.</exception>
        /// <exception cref="System.NotSupportedException">
        ///     An attempt was made to use unsupported behavior such as executing multiple
        ///     asynchronous commands concurrently on the same context instance.</exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     The context or connection have been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Some error occurred attempting to process entities in the context either
        ///     before or after sending commands to the database.</exception>
        /// <seealso cref="DbContext.SaveChangesAsync"/>
        /// <returns>A task that represents the asynchronous save operation.  The
        ///     <see cref="Task.Result">Task.Result</see> contains the number of
        ///     objects written to the underlying database.</returns>
        public override async Task<int> SaveChangesAsync()
        {
            return await SaveChangesAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <seealso cref="DbContext.SaveChangesAsync"/>
        /// <returns>A task that represents the asynchronous save operation.  The
        ///     <see cref="Task.Result">Task.Result</see> contains the number of
        ///     objects written to the underlying database.</returns>
        /// <exception cref="System.Data.Entity.Infrastructure.DbUpdateException">
        ///     An error occurred sending updates to the database.</exception>
        /// <exception cref="System.Data.Entity.Infrastructure.DbUpdateConcurrencyException">
        ///     A database command did not affect the expected number of rows. This usually
        ///     indicates an optimistic concurrency violation; that is, a row has been changed
        ///     in the database since it was queried.</exception>
        /// <exception cref="System.Data.Entity.Validation.DbEntityValidationException">
        ///     The save was aborted because validation of entity property values failed.</exception>
        /// <exception cref="System.NotSupportedException">
        ///     An attempt was made to use unsupported behavior such as executing multiple
        ///     asynchronous commands concurrently on the same context instance.</exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     The context or connection have been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Some error occurred attempting to process entities in the context either
        ///     before or after sending commands to the database.</exception>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                SyncObjectsStatePreCommit();
                var changesAsync = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
                SyncObjectsStatePostCommit();
                return changesAsync;
            }
            catch (DbEntityValidationException ex)
            {
                throw new BusinessException(ErrorCode.GLB106, ex.EntityValidationErrors);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new BusinessException(ex);
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException(ex);
            }
            catch (DbException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Synchronizes the objects state post commit.
        /// </summary>
        public void SyncObjectsStatePostCommit()
        {
            var doSave = false;
            
            foreach (
                var dbEntityEntry in
                    ChangeTracker.Entries()
                        .Where(dbEntityEntry => (dbEntityEntry.Entity is TrackoApi.Models.Base.Entity)))
            {
                ObjectState objectState = ObjectState.Unchanged;
                try
                {
                    //if (entity.GetType() == typeof(CnChallan))
                    //{
                    //    Debugger.Break();
                    //}
                    if (dbEntityEntry.Entity is IEntity entity)
                    {
                        objectState = entity.ObjectState;
                        entity.ObjectState = StateHelper.ConvertState(dbEntityEntry.State);
                        if (dbEntityEntry.State == EntityState.Modified && entity.ObjectState == ObjectState.Unchanged)
                        {
                            this.Entry(dbEntityEntry.Entity).State = EntityState.Unchanged;
                        }
                    }
                    var result = PostCoreLogic(dbEntityEntry);
                    doSave = doSave || result;
                    //dbEntityEntry.State = StateHelper.ConvertState(((IEntity)dbEntityEntry.Entity).ObjectState);

                    //Debug.Assert(entity != null);
                }
                catch (Exception)
                {
                    throw;
                }
                if (NotificationRulesAreOn == null)
                {
                    NotificationRulesAreOn = GetApiConfig<int>("RunAPINotificationRules") == 1;
                }
                
                if (NotificationRulesAreOn.GetValueOrDefault(false))
                {
                    switch (objectState)
                    {
                        case ObjectState.Added:
                        case ObjectState.Deleted:
                        case ObjectState.Modified:

                            var rulekey = $"NotifyOn{dbEntityEntry.Entity.GetType().Name}{objectState}";
                            try
                            {
                                var rules = this.Set<Rule>().Where(x => x.IsActive && x.RuleKey == rulekey)
                                    .FromCache(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)))
                                    ?.ToList();
                                if (rules != null)
                                {
                                    if (rules.Any())
                                    {

                                        var entitytype = dbEntityEntry.Entity as TrackoApi.Models.Base.Entity;
                                        var logics2Apply = rules.Where(x =>
                                            x.RuleNature == RuleNature.Assignment &&
                                            (string.IsNullOrWhiteSpace(x.ValidationDefination) ||
                                             dbEntityEntry.VaidateDbEntry(entitytype, this, x.ValidationDefination, x.Id, this.DbName)));
                                        JobLog job = null;
                                        foreach (var rule in logics2Apply)
                                        {
                                            if (job == null)
                                            {
                                                job = new JobLog
                                                {
                                                    LastJobStatus = JobResult.Pending,
                                                    JobNatureId = 1507,
                                                    IntervalTypeId = 1495,
                                                    JobName = $"{rulekey}_{(entitytype.GetPropertyValue("Id") ?? 0)}",
                                                    MessageType = NotificationType.SMS,
                                                    StartDate = DateTime.Now,
                                                    _ExtendedInfo = JsonConvert.SerializeObject(
                                                        new
                                                        {
                                                            Id = entitytype.GetPropertyValue("Id"),
                                                            TypeName = dbEntityEntry.Entity.GetType().Name,
                                                            ActionType = objectState.ToString(),
                                                            CSID = entitytype.GetPropertyValue("CreatedSessionId"),
                                                            RuleId = rule.Id
                                                        }
                                                    ),
                                                    CreatedSessionId = SessionId,
                                                    SecuredByTenantId = TenantId,
                                                    CreatedDOE = DateTime.Now
                                                };
                                            }
                                            if (!string.IsNullOrWhiteSpace(rule.AssignmentDefination))
                                            {
                                                job.ApplyDbRule(entitytype,this, rule.AssignmentDefination, rule.Id, this.DbName);
                                            }
                                        }
                                        if (job != null)
                                        {                                            
                                            this.ExecuteProcedure("[dbo].[Proc_GLB_ScheduleJob]", new SqlParameter("jsonJob", JsonConvert.SerializeObject(job)));
                                        }
                                    }
                                }
                            }
                            catch (BusinessException)
                            {
                                throw;
                            }
                            catch (Exception)
                            {
                                //Ignore
                            }

                            break;
                    }
                }
            }

            if (!doSave) return;            
            UpdateSessionInfo();
            this.SaveChanges();
            PostDataCommit();
        }
        public string DbName => this.Database.Connection.Database;
        public async Task<string> ValidateTLDateRangeOverlap(DateTime tripStartDate, DateTime? tripEndDate, long ownvehicleid = 0, long hirevehicleid = 0, long triplogId = 0, long triptype = 1158, long tripnature = 0)
        {
            try
            {
                return await this.Database.SqlQuery<string>($"SELECT [dbo].[ValidateTLDateRangeOverlap]('{tripStartDate.ToString("yyyy-MM-dd HH:mm:ss")}','{(tripEndDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")}',{ownvehicleid},{hirevehicleid},{triplogId},{triptype},{tripnature})")
                    .FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                return e.GetBaseException().Message;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    // free other managed objects that implement
                    // IDisposable only
                }

                // release any unmanaged objects
                // set object references to null

                Disposed = true;
            }

            base.Dispose(disposing);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApiRole>()
                .ToTable("ApiRoles")
                .HasMany(x => x.AccessList)
                .WithRequired(x => x.ApiRole)
                .HasForeignKey(x => x.ApiRoleId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<ConversationGroup>().HasMany(x => x.Users).WithMany(x => x.Groups).Map(cs =>
            {
                cs.MapLeftKey("UserId");
                cs.MapRightKey("GroupId");
                cs.ToTable("ApiUserGroupMapping");
            });
            modelBuilder.Entity<UserConnection>().HasRequired(x => x.fk_User).WithMany(x => x.Connections).HasForeignKey(x => x.UserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ApiUserClaim>().ToTable("ApiUserClaims");
            modelBuilder.Entity<ApiUserLogin>().ToTable("ApiUserLogins");
            modelBuilder.Entity<ApiUserRole>().ToTable("ApiUserRoles");
            modelBuilder.Conventions.Add(new DataTypePropertyAttributeConvention());
            modelBuilder.Conventions.Add(new FunctionsConvention("dbo", typeof(MyDbFunctions)));
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<XmlSqlType, string>("XmlSqlType", (p, attributes) => "xml"));
            Precision.ConfigureModelBuilder(modelBuilder);
        }

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {
            var result = new DbEntityValidationResult(entityEntry,
                new System.Collections.Generic.List<DbValidationError>());

            //CoreLogic(entityEntry);
            //TODO:Write Core Logic Over here
            if (entityEntry.Entity is Voucher voucher)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    if (this.Set<Voucher>().Count(x => x.VoucherNo == voucher.VoucherNo) > 0)
                    {
                        result.ValidationErrors.Add(new DbValidationError("VoucherNo",
                            $"Core:Transaction with Voucher No {voucher.VoucherNo} already exist."));
                    }

                    if (voucher.FinancialYearId.GetValueOrDefault(0) <= 0)
                    {
                        CheckVoucher(entityEntry);
                    }
                }
            }
            else if (entityEntry.Entity is PostalAddress add)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(add.FullAddress))
                    {
                        add.FullAddress = string.Empty;
                        if (!string.IsNullOrWhiteSpace(add.UnitNo))
                        {
                            add.FullAddress += $"{add.UnitNo}";
                        }
                        if (!string.IsNullOrWhiteSpace(add.AddressLine1))
                        {
                            add.FullAddress += $", {add.AddressLine1}";
                        }
                        if (!string.IsNullOrWhiteSpace(add.AddressLine2))
                        {
                            add.FullAddress += $", {add.AddressLine2}";
                        }
                        if (!string.IsNullOrWhiteSpace(add.AddressLine3))
                        {
                            add.FullAddress += $", {add.AddressLine3}";
                        }
                        if (!string.IsNullOrWhiteSpace(add.Landmark))
                        {
                            add.FullAddress += $",{add.Landmark}";
                        }

                        if (add.CityId.HasValue)
                        {
                            if (add.fk_City == null)
                            {
                                add.FullAddress +=
                                    $", {this.Set<CityMaster>().Where(x => x.Id == add.CityId).Select(x => x.CityName).FirstOrDefault()}";
                            }
                            else
                            {
                                add.FullAddress +=
                                    $", {add.fk_City.CityName}";
                            }
                            if (!string.IsNullOrWhiteSpace(add.PostalCode))
                            {
                                add.FullAddress += $"-{ add.PostalCode}";
                            }
                        }
                        if (add.StateId.HasValue)
                        {
                            if (add.fk_State == null)
                            {
                                add.FullAddress +=
                                    $", {this.Set<GenericMaster>().Where(x => x.Id == add.StateId).Select(x => x.Name).FirstOrDefault()}";
                            }
                            else
                            {
                                add.FullAddress +=
                                    $", {add.fk_State.Name}";
                            }
                        }
                        if (add.CountryId.HasValue)
                        {
                            if (add.fk_State == null)
                            {
                                add.FullAddress +=
                                    $", {this.Set<Country>().Where(x => x.Id == add.CityId).Select(x => x.CountryName).FirstOrDefault()}";
                            }
                            else
                            {
                                add.FullAddress +=
                                    $",{add.fk_Country.CountryName}";
                            }
                        }
                    }
                    this.Entry(add);
                }
                catch (Exception)
                {
                    //Ignore
                }
            }
            if (entityEntry.State == EntityState.Modified || entityEntry.State == EntityState.Deleted)
            {
                try
                {
                    if (entityEntry.Entity.GetPropertyValue("IsReserved") != null) { 
                    var member = entityEntry.Member("IsReserved");
                    if (member != null)
                    {
                        var dbvalues = entityEntry.GetDatabaseValues();
                        if (dbvalues == null)
                        {
                            result.ValidationErrors.Add(new DbValidationError("IsReserved", "Core:Invalid Entity Type"));
                        }
                        else
                        {
                            if ((bool)dbvalues["IsReserved"])
                            {
                                result.ValidationErrors.Add(new DbValidationError("IsReserved",
                                    "Core:System Reserved Entity is Not Allowed to Update/Delete."));
                            }
                        }
                    }
                    }
                }
                catch (Exception)
                {
                    //Ignore
                }
            }
            return (result.ValidationErrors.Count > 0) ? result : base.ValidateEntity(entityEntry, items);
        }
        private void DeleteApprovalLogs(string Key,long RecordId)
        {
            var js = Set<JsonTransactionLog>()
                    .Where(x => x.Key == Key && x.RecordId == RecordId)
                    .FirstOrDefault();
            if (js != null)
            {
                js.ObjectState = ObjectState.Deleted;

                var apl = this.Set<APLLog>()
                        .Where(x => x.APLRequestId == js.Id).FirstOrDefault();
                if (apl != null)
                {
                    apl.ObjectState = ObjectState.Deleted;
                }
            }
        }
        
        private void CheckVoucher(DbEntityEntry entry, bool forceValidation = false)
        {
            #region Voucher Logic
            var voucher = entry.Entity as Voucher;
            if (voucher == null) return;

            #region Currency Conversion
            try
            {
                voucher.ConstCurTypeId = voucher.ConstCurTypeId ?? Helper.ConstCurTypeId;
                if (voucher.CurTypeId != voucher.ConstCurTypeId && voucher.CurRate <= 0)
                {
                    var _result =
                    this.Set<CurrencyConversion>().Where(x => x.IsActive && x.CurDate <= voucher.VoucherDate && x.CurTypeId == voucher.CurTypeId)
                    .OrderByDescending(p => p.CurDate)
                    .Select(x => new { x.CurRate }).FirstOrDefault();
                    voucher.CurRate = _result?.CurRate ?? 1;
                }

                voucher.CurRate = voucher.CurRate <= 0 ? 1 : voucher.CurRate;
                voucher.VoucherAmount_MNC = voucher.IsCCRequired || voucher.VoucherAmount_MNC == 0 ? voucher.VoucherAmount * (1 * voucher.CurRate) : voucher.VoucherAmount_MNC;
                voucher.Amount1_MNC = voucher.IsCCRequired || voucher.Amount1_MNC == 0 ? voucher.Amount1 * (1 * voucher.CurRate) : voucher.Amount1_MNC;
                voucher.Amount2_MNC = voucher.IsCCRequired || voucher.Amount2_MNC == 0 ? voucher.Amount2 * (1 * voucher.CurRate) : voucher.Amount2_MNC;
                voucher.Amount3_MNC = voucher.IsCCRequired || voucher.Amount3_MNC == 0 ? voucher.Amount3 * (1 * voucher.CurRate) : voucher.Amount3_MNC;
                voucher.Amount4_MNC = voucher.IsCCRequired || voucher.Amount4_MNC == 0 ? voucher.Amount4 * (1 * voucher.CurRate) : voucher.Amount4_MNC;
                voucher.Amount5_MNC = voucher.IsCCRequired || voucher.Amount5_MNC == 0 ? voucher.Amount5 * (1 * voucher.CurRate) : voucher.Amount5_MNC;
                voucher.Amount6_MNC = voucher.IsCCRequired || voucher.Amount6_MNC == 0 ? voucher.Amount6 * (1 * voucher.CurRate) : voucher.Amount6_MNC;
                voucher.Amount7_MNC = voucher.IsCCRequired || voucher.Amount7_MNC == 0 ? voucher.Amount7 * (1 * voucher.CurRate) : voucher.Amount7_MNC;
                voucher.Amount8_MNC = voucher.IsCCRequired || voucher.Amount8_MNC == 0 ? voucher.Amount8 * (1 * voucher.CurRate) : voucher.Amount8_MNC;
                voucher.Amount9_MNC = voucher.IsCCRequired || voucher.Amount9_MNC == 0 ? voucher.Amount9 * (1 * voucher.CurRate) : voucher.Amount9_MNC;
                voucher.Amount10_MNC = voucher.IsCCRequired || voucher.Amount10_MNC == 0 ? voucher.Amount10 * (1 * voucher.CurRate) : voucher.Amount10_MNC;

            }
            catch
            {
                //nothing to do 
            }
            
            #endregion

            if (voucher.GroupVoucherId.GetValueOrDefault(0)>0&& this.Set<Voucher>().Any(x=>x.Id==voucher.GroupVoucherId.GetValueOrDefault()))
            {
                throw new BusinessException(ErrorCode.VCH103, "Cannot update or delete this transaction as Group Voucher has been created");
            }
            if (voucher.ObjectState != ObjectState.Unchanged)
            {
                forceValidation = false;
            }
            var vchconfig =
                this.Set<VoucherType>()
                    .Where(x => x.Id == voucher.VoucherTypeId)
                    .Select(x => new { x.IsAccountSubscribed, x.IsApprovalRequired })
                    .FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)));

            bool subuscribed = vchconfig?.IsAccountSubscribed ?? false, approvalrequired = vchconfig?.IsApprovalRequired ?? false;
            if (voucher == null || (!forceValidation && voucher.ObjectState == ObjectState.Unchanged))
            {
                return;
            }
            //Apply Financial Year Id and Check whether Voucher should be Shown on Bearer or should directly imported in accounts

            if (voucher.Id > 0 && (voucher.ObjectState == ObjectState.Deleted || voucher.ObjectState == ObjectState.Modified || forceValidation))
            {
                bool originalIsAccpetedFlag = (bool)entry.OriginalValues["IsAccepted"];
                bool originalIsAuditedFlag = (bool)entry.OriginalValues["IsAudited"];
                if (originalIsAccpetedFlag && subuscribed && approvalrequired)
                {
                    throw new BusinessException(ErrorCode.VCH101); //Cannot Modify Accepted Transaction
                }
                if (originalIsAuditedFlag && subuscribed)
                {
                    throw new BusinessException(ErrorCode.VCH102);
                    //The Audited Voucher Transaction cannot be deleted.
                }
            }
            if (voucher.VoucherDetails != null)
            {
                foreach (VoucherDetail detail in voucher.VoucherDetails)
                {
                    detail.VoucherDetailReferences.RemoveAll(x => x.ObjectState == ObjectState.Deleted);
                }
                voucher.VoucherDetails.RemoveAll(x => x.ObjectState == ObjectState.Deleted);
            }
            var fy =
                this.FinancialYears.Where(
                    x =>
                        x.OpeningDate <= voucher.VoucherDate && x.ClosingDate >= voucher.VoucherDate &&
                        x.IsActive).Select(x => new { x.Id, x.IsLocked, x.Name, x.LockUpToDate }).FirstOrDefault();
            voucher.FinancialYearId = fy?.Id;
            if (!subuscribed)
            {
                if (!forceValidation)
                {
                    voucher.FinancialYearId = null;
                    voucher.IsAccepted = false;
                    voucher.IsAccountsVisiblity = false;
                    voucher.IsAudited = false;
                }
            }
            else
            {
                if (fy == null)
                {
                    if (!this.FinancialYears.Any())
                    {
                        throw new BusinessException(ErrorCode.GLB107, "Financial Year Not Created");
                    }
                    throw new BusinessException(ErrorCode.VCH100);
                    //The Voucher date is out of system scope range.
                }
                //var fydlock = this.Set<FinancialYearLedgerLockLog>().Where(x => x.FinancialYearId == fy.Id&&x.LedgerId==).FromCacheFirstOrDefault(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(2)));
                if (fy.IsLocked || (fy?.LockUpToDate != null && fy?.LockUpToDate?.Date >= voucher.VoucherDate.Date))
                {
                    throw new BusinessException(ErrorCode.VCH112,
                        $"The Financial Year '{fy.Name}' is {(fy.IsLocked ? "fully " : "")} Locked {(!fy.IsLocked && fy?.LockUpToDate != null ? $" till {fy.LockUpToDate?.ToString("dd-MMM-yyyy HH:mm")}" : "")}");
                }

                if (!forceValidation)
                {
                    if (approvalrequired)
                    {
                        voucher.FinancialYearId = fy?.Id;
                        voucher.IsAccepted = false;
                        voucher.IsAccountsVisiblity = true;
                        voucher.IsAudited = false;
                    }
                    else
                    {
                        voucher.FinancialYearId = fy?.Id;
                        voucher.IsAccepted = true;
                        voucher.IsAccountsVisiblity = true;
                        voucher.IsAudited = false;
                    }
                }
            }
            if (voucher.ObjectState == ObjectState.Modified && voucher.IsAudited)
            {
                bool originalIsAuditedFlag = (bool)entry.OriginalValues["IsAudited"];
                if (originalIsAuditedFlag)
                {
                    voucher.AuditSessionId = Helper.SessionId();

                }
            }
            //var rules = this.Set<Rule>().Where(x => x.RuleKey == "vouchercud")
            //    .FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(9)))
            //    .CompileFromCache<Voucher>();
            //if (rules.TrueForAll(rule => rule(voucher)))
            //{
            //    throw new BusinessException(ErrorCode.GLB106, "Validation Failed For Voucher Creation Process");
            //}

            //var rulesn = this.Set<Rule>().Where(x => x.RuleKey == "vouchercud")
            //    .FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(9)))
            //    .CompileSingleFromCache<Voucher>();
            //fore
            //if (rulesn(voucher))
            //{
            //    throw new BusinessException(ErrorCode.GLB106, "Validation Failed For Voucher Creation Process");
            //}
            //var voucherapproval_rules = this.Set<Rule>().Where(x => x.RuleKey == "voucherapproval")
            //    .FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(6)))
            //    .CompileFromCache<Voucher>();
            //if (voucherapproval_rules.TrueForAll(rule => rule(voucher)))
            //{
            //    voucher.IsAccepted = false;
            //    voucher.IsAccountsVisiblity = true;
            //    voucher.IsAudited = false;
            //}
            if (!forceValidation)
            {
                if (voucher.IsAccepted && approvalrequired)
                {
                    voucher.IsAccepted = false;
                    voucher.IsAccountsVisiblity = true;
                    voucher.IsAudited = false;
                }
                else if (!voucher.IsAccepted && !approvalrequired)
                {
                    voucher.IsAccepted = true;
                    voucher.IsAccountsVisiblity = true;
                    voucher.IsAudited = false;
                }
            }

            #endregion Voucher Logic
        }

        private EntitySet GetEntitySet(ObjectContext context, Type type, out Type setType)
        {
            //source url http://www.seguetech.com/performing-bulk-updatesentity-framework-6-1/
            //By Default, the setType is the same as we typed in.
            //It's different in TPH
            setType = type;
            var metadata = context.MetadataWorkspace;
            //the ObjectItemCollection maps EntityTypes to CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));
            //find the EntityType which maps to the CLR type which we passed in.
            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace).Single(e => objectItemCollection.GetClrType(e) == type);
            //find the EntitySet for EntityType
            var entitySet =
                metadata.GetItems(DataSpace.CSpace).Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType).Cast<EntityType>().Single(x => x.Name == entityType.Name);
            //Get all the mappings (table, columns, etc.) for the Entity Set
            var entitySetMappings =
                metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings.ToList();
            EntitySet table;
            //In Simplest case, there would be single mapping for the entity set
            var mapping = entitySetMappings.SingleOrDefault(x => x.EntitySet.Name == entitySet.Name);
            if (mapping != null)
            {
                //if this is the simplest case, get the store entity set from the mapping
                table = mapping.EntityTypeMappings.Single().Fragments.Single().StoreEntitySet;
            }
            else
            {
                //I the case of TPH and TPI, The Entity Set we are looking for may not be for
                //the type we passed in.
                //To be honest I'm not sure which scenario each of the following conditions covers!
                mapping =
                    entitySetMappings.SingleOrDefault(
                        x =>
                            x.EntityTypeMappings.Where(y => y.EntityType != null)
                                .Any(y => y.EntityType.Name == entitySet.Name));
                if (mapping != null)
                {
                    table =
                        mapping.EntityTypeMappings.Where(x => x.EntityType != null)
                            .Single(x => x.EntityType.Name == entityType.Name)
                            .Fragments.Single()
                            .StoreEntitySet;
                }
                else
                {
                    var entitySetMapping =
                        entitySetMappings.Single(
                            x => x.EntityTypeMappings.Any(y => y.IsOfEntityTypes.Any(z => z.Name == entitySet.Name)));
                    table =
                        entitySetMapping.EntityTypeMappings.First(
                            x => x.IsOfEntityTypes.Any(y => y.Name == entitySet.Name)).Fragments.Single().StoreEntitySet;
                }
                //Assuming we found an Entity Set, figure out the CLR type the Entity Set is for
                if (table != null)
                {
                    foreach (var setEntityType in metadata.GetItems<EntityType>(DataSpace.OSpace))
                    {
                        //Get the  entity set for the entity type
                        entitySet =
                            metadata.GetItems(DataSpace.CSpace)
                                .Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                                .Cast<EntityType>()
                                .Single(x => x.Name == setEntityType.Name);
                        //Get the mappings for the entity set
                        entitySetMappings =
                            metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                .Single()
                                .EntitySetMappings.ToList();
                        //Find the mapping where the Entity Set for the mapping matches the Entity Set name for the Entity Set we are returning
                        mapping =
                            entitySetMappings.SingleOrDefault(
                                x =>
                                    x.EntitySet.Name == entitySet.Name &&
                                    x.EntityTypeMappings.Any(m => m.Fragments.Single().StoreEntitySet == table));
                        if (mapping != null)
                        {
                            //if there is one, the CLR type for the entity type we are checking is the type for the Entity Set we're returning
                            setType = objectItemCollection.GetClrType(setEntityType) ?? type;
                            break;
                        }
                    }
                }
            }
            return table;
        }

        private MappingAPI GetMapping(ObjectContext context, Type type)
        {
            return _globalStore.MappingCache.GetOrAdd(type, tp =>
            {
                Type setType;
                var entitySet = GetEntitySet(context, type, out setType);
                //Create a Mapping Instance to add to the cache

                return new MappingAPI()
                {
                    EntitySet = entitySet,
                    SetType = setType,
                    //Extract the TableName(Including schema) for easy access
                    TableName = $"[{entitySet.MetadataProperties["Schema"].Value ?? entitySet.Schema ?? "dbo"}].[{entitySet.MetadataProperties["Table"].Value ?? entitySet.Name}]",
                    //Extract the Primary Keys for easy access
                    KeyMembers = entitySet.ElementType.KeyMembers
                };
            });
            ////if the type we'er looking for has already been cached, retrive the cached mapping
            //if (GlobalStore.Instance.MappingCache.ContainsKey(type))
            //{
            //    return GlobalStore.Instance.MappingCache[type];
            //}
            //else
            //{
            //    //Retrive the Entity  Set and Set Type (supports TPH) for type
            //    Type setType;
            //    var entitySet = GetEntitySet(context, type, out setType);
            //    //Create a Mapping Instance to add to the cache

            //    var cacheItem = new MappingAPI()
            //    {
            //        EntitySet = entitySet,
            //        SetType = setType,
            //        //Extract the TableName(Including schema) for easy access
            //        TableName = $"[{entitySet.MetadataProperties["Schema"].Value ?? entitySet.Schema ?? "dbo"}].[{entitySet.MetadataProperties["Table"].Value ?? entitySet.Name}]",
            //        //Extract the Primary Keys for easy access
            //        KeyMembers = entitySet.ElementType.KeyMembers
            //    };
            //    //Add the MappingAPI Instance to the cache, and return it

            //    MappingCache[type] = cacheItem;
            //    return cacheItem;
            //}
        }

        private InnerSelect GetSelectSql<TEntity>(ObjectQuery<TEntity> query, ICollection<EdmMember> keys) where TEntity : class
        {
            var innerSelect = new InnerSelect();
            string selector = string.Format("new({0})", string.Join(",", keys.Select(x => x.Name)));
            var selectQuery = System.Linq.Dynamic.DynamicQueryable.Select(query, selector) as ObjectQuery;
            innerSelect.Sql = selectQuery.ToTraceString();
            foreach (var objectParam in selectQuery.Parameters)
            {
                var param = new SqlParameter();
                param.ParameterName = objectParam.Name;
                param.Value = objectParam.Value ?? DBNull.Value;
                innerSelect.Parameters.Add(param);
            }
            return innerSelect;
        }
        private string PopulateTenantId()
        {
            string tenantId =null;
            try
            {
                tenantId = Helper.LoggedInTenantId;                
            }
            catch (Exception)
            {
                //Ignore
            }
            return tenantId;
        }
        private long PopulateSessionId()
        {
            long sessionId = 0;
            try
            {
                if (HttpContext.Current?.User == null) sessionId = 0;
                var ctx = (ClaimsPrincipal)HttpContext.Current?.User;
                var sessionIdObj = ctx?.Claims.FirstOrDefault(x => x.Type == "SessionId");
                sessionId = sessionIdObj == null ? 0 : long.Parse(sessionIdObj.Value);
            }
            catch (Exception)
            {
                //Ignore
            }
            return sessionId;
        }
        private bool PostCoreLogic(DbEntityEntry entry)
        {
            bool doSave = false;
            switch (entry.Entity.GetType().Name)
            {
                case "JobLog":
                    new JobLogCoreLogic().Bind(this).Execute(entry, true);
                    break;

                case "CnChallan":
                    try
                    {
                        var chcn = new CnChallanCoreLogic().Bind(this);
                        chcn.Execute(entry, true);
                        doSave = chcn.SaveAfterPostLogic;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    break;

                case "CNDTSStatusLog":
                    var cndts = new CNDTSStatusCoreLogic().Bind(this);
                    cndts.Execute(entry, true);
                    doSave = cndts.SaveAfterPostLogic;
                    break;
                case "CNStockLog":
                    var cnstk = new CNStockLogCoreLogic().Bind(this);
                    cnstk.Execute(entry, true);
                    doSave = cnstk.SaveAfterPostLogic;
                    break;
                case "TripAdvanceLog":
                    var tallogic = new TripAdvanceCoreLogic().Bind(this);
                    tallogic.Execute(entry, true);
                    doSave = tallogic.SaveAfterPostLogic;
                    break;
                case "Voucher":
                    var v = new VoucherCoreLogic().Bind(this);
                    v.Execute(entry, true);
                    doSave = v.SaveAfterPostLogic;
                    break;
                
            }
            return doSave;
        }

        private void PreCoreLogic(DbEntityEntry entry)
        {
            switch (entry.Entity.GetType().Name)
            {
                case "PurchaseRequisition":
                    var pl = entry.Entity as PurchaseRequisition;
                    break;
                
                case "PurchaseOrder":
                    var po = entry.Entity as PurchaseOrder;
                    break;
                
                case "Voucher":
                    CheckVoucher(entry);
                    break;
                
                case "VoucherDetail":
                    var vd = entry.Entity as VoucherDetail;
                    vd.ConstCurTypeId = Helper.ConstCurTypeId;
                    if (vd != null)
                    {
                        if (vd.Voucher != null)
                        {
                            var query = this.Set<FinancialYearLedgerLockLog>().Where(x => x.FinancialYearId == vd.Voucher.FinancialYearId && x.LedgerId == vd.AccountId);
                            if (query.Any())
                            {
                                var fydlock = query.Select(x => new { x.fk_Ledger.AccountName, x.LockedDate, FyDate = x.fk_FinancialYear.OpeningDate }).FromCacheFirstOrDefault(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(8)));
                                if (fydlock?.LockedDate != null && fydlock.LockedDate.Date >= vd.Voucher.VoucherDate.Date)
                                {
                                    throw new BusinessException(ErrorCode.VCH114, $"Account \"{fydlock.AccountName}\" has been locked from {fydlock.FyDate} till {fydlock.LockedDate}");
                                }
                            }
                        }
                    }
                    #region Currency Conversion
                    try
                    {
                        if (vd.IsCCRequired)
                        {
                            vd.ConstCurTypeId = Helper.ConstCurTypeId;
                            vd.CurTypeId = vd.CurTypeId ?? (vd.Voucher != null ? vd.Voucher.CurTypeId : Helper.ConstCurTypeId);
                            vd.CurRate = vd.CurRate > 0 ? vd.CurRate : (vd.Voucher != null ? vd.Voucher.CurRate : 0);
                            if (vd.CurTypeId != vd.ConstCurTypeId && vd.CurRate <= 0)
                            {
                                var _result =
                                this.Set<CurrencyConversion>().Where(x => x.IsActive && x.CurDate <= (vd.Voucher!=null? vd.Voucher.VoucherDate:DateTime.Now) && x.CurTypeId == vd.Voucher.CurTypeId)
                                .OrderByDescending(p => p.CurDate)
                                .Select(x => new { x.CurRate }).FirstOrDefault();
                                vd.CurRate = _result?.CurRate ?? 0;
                            }
                            /*Currency Amount is Overrite by Proc_GBL_Save_FxInVch in case Against REF VD*/
                            vd.Amount_MNC = Math.Round(vd.CurRate * vd.Amount,2);
                            vd.Amount1_MNC = Math.Round(vd.CurRate * vd.Amount1,2);                            
                        }
                    }
                    catch
                    {
                        //nothing to do 
                    }
                    
                    #endregion
                    //this.Database.ExecuteSqlCommand("UPDATE V SET VDCount=ISNULL((SELECT count(1) from tVoucherVD vd WHERE vd.VoucherId=V.Id),0) FROM [dbo].[tVouchers] V WHERE V.Id=@id", new SqlParameter("id", vd.VoucherId));
                    break;
                case "VoucherDetailReference":
                    var vdr = entry.Entity as VoucherDetailReference;
                    var vdrdbset = this.Set<VoucherDetailReference>();
                    if (vdr == null || vdr.ObjectState == ObjectState.Unchanged) break;
                    vdr.ConstCurTypeId = Helper.ConstCurTypeId;
                    if (vdr.Id > 0)
                    {
                        if (vdr.ObjectState == ObjectState.Deleted)
                        {
                            var provisionedToDelete = ChangeTracker.Entries<VoucherDetailReference>().Where(x => (x.State == EntityState.Deleted || x.Entity.ObjectState == ObjectState.Deleted) && x.Entity.Id != vdr.Id).Select(x => x.Entity.Id).ToArray();
                            var provisionedVoucherDeleted = ChangeTracker.Entries<Voucher>().Where(x => (x.State == EntityState.Deleted || x.Entity.ObjectState == ObjectState.Deleted)).Select(x => x.Entity.Id).ToArray();
                            var existingrefids = vdrdbset.Count(x => x.RefId == vdr.Id && (!provisionedToDelete.Contains(x.Id) && !provisionedVoucherDeleted.Contains(x.fk_VoucherDetail.VoucherId)));
                            if (existingrefids > 0)
                            {
                                throw new BusinessException(ErrorCode.VCH103, $"Reference No:{vdr.ReferenceNo} has been reference somewhere."); //Used Reference cannot be deleted
                            }
                            break;
                        }
                    }

                    if (vdr.AccountId.GetValueOrDefault() <= 0 && vdr.fk_VoucherDetail != null)
                    {
                        vdr.AccountId = vdr.fk_VoucherDetail.AccountId;
                    }
                    if (vdr.fk_VoucherDetail?.Voucher != null && vdr.DueDate == default(DateTime))
                    {
                        if (new long[] { 1013, 1448, 1449 }.Contains(vdr.VDRTypeId))
                        {
                            var creditperiod = this.Set<Ledger>().Where(x => x.Id == vdr.AccountId)
                                                   .Select(x => new { x.CreditPeriod }).FromCacheFirstOrDefault(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)))?.CreditPeriod ?? 0;
                            vdr.DueDate = vdr.fk_VoucherDetail.Voucher.VoucherDate.AddDays(creditperiod);
                        }
                        else
                        {
                            vdr.DueDate = vdr.fk_VoucherDetail.Voucher.VoucherDate;
                        }
                    }
                    if (vdr.fk_ParentReference != null && vdr.fk_ParentReference.ObjectState == ObjectState.Added && vdr.fk_ParentReference.Id == 0)
                    {
                        break;
                    }
                    if (vdr.VDRTypeId == 1014 && vdr.RefId.GetValueOrDefault() <= 0 && (vdr.AccountId > 0 || vdr.fk_VoucherDetail != null))
                    {
                        var lst = new List<long> { 1449, 1013 };
                        var accountid = vdr.fk_VoucherDetail?.AccountId ?? vdr.AccountId;
                        var pvdr = vdrdbset.FirstOrDefault(x => x.ReferenceNo == vdr.ReferenceNo && (accountid == x.AccountId || accountid == x.fk_VoucherDetail.AccountId) && lst.Contains(x.VDRTypeId));
                        if (pvdr == null)
                        {
                            pvdr = ChangeTracker.Entries<VoucherDetailReference>().FirstOrDefault(x => x.Entity.ReferenceNo == vdr.ReferenceNo && (accountid == x.Entity.AccountId || accountid == x.Entity.fk_VoucherDetail.AccountId) && lst.Contains(x.Entity.VDRTypeId))?.Entity;
                        }
                        vdr.RefId = pvdr?.Id;
                        vdr.fk_ParentReference = pvdr;
                    }
                    
                    if (vdr.VDRTypeId == 1014 && vdr.RefId.GetValueOrDefault() <= 0)
                    {
                        throw new BusinessException(ErrorCode.VCH109,
                            $"Core:Missing Reference Number against which payment has made.\nRefNo:{vdr.ReferenceNo} Payment Rs.{vdr.Amount.ToString(CultureInfo.InvariantCulture)} AccountId {vdr.AccountId}");
                    }

                    if (vdr.RefId > 0 && vdr.ObjectState!=ObjectState.Deleted)
                    {
                        var parentVDR =
                            vdrdbset.Where(x => x.Id == vdr.RefId)
                                .Select(x => new { x.CurRate, x.CurTypeId, x.Amount, x.Amount_MNC, x.OriginalRefId, x.VDRTypeId, VoucherTypeId = x.fk_VoucherDetail.Voucher.VoucherTypeId })
                                .FirstOrDefault();
                        if (parentVDR?.VoucherTypeId != 89/*Freight Sales*/)
                        {
                            if (parentVDR == null)
                            {
                                throw new BusinessException(ErrorCode.VCH107);
                            }
                            vdr.OriginalRefId = parentVDR.OriginalRefId;

                            if (parentVDR.CurTypeId == Helper.ConstCurTypeId && vdr.CurTypeId != Helper.ConstCurTypeId)
                            {
                                vdr.OldCurRate = vdr.CurRate;
                            }
                            else
                            {
                                vdr.OldCurRate = vdr.OldCurRate > 0 ? vdr.OldCurRate : parentVDR?.CurRate ?? 0;
                            }

                            if (vdr.IsCCRequired)
                            {   
                                vdr.CurRate = (vdr.CurRate > 0 ? vdr.CurRate : (decimal)1);
                                vdr.CurTypeId = vdr.CurTypeId ?? parentVDR?.CurTypeId;

                                if (vdr.CurTypeId != Helper.ConstCurTypeId && vdr.CurRate<=1) {
                                    vdr.CurRate = vdr.fk_VoucherDetail.CurRate;
                                }
                            }
                            //if(parentVDR.VDRTypeId== 1013)//Parent is opening
                            var parentAmt = parentVDR.Amount_MNC;
                            if (vdr.IsCCRequired)
                            {
                                if (vdr.OldCurRate <= 0)
                                {
                                    vdr.OldCurRate = parentVDR.CurTypeId == Helper.ConstCurTypeId && vdr.CurTypeId != Helper.ConstCurTypeId ? vdr.CurRate : vdr.OldCurRate;
                                }
                                vdr.Amount_MNC = (vdr.Amount + parentVDR.Amount) == 0 ? -1 * parentVDR.Amount_MNC : Math.Round(vdr.OldCurRate * vdr.Amount, 2);
                            }

                            if (vdr.CurTypeId == parentVDR.CurTypeId)
                            {
                                var childsum =
                                    vdrdbset.Where(
                                            x =>
                                                x.RefId == vdr.RefId && x.Id != vdr.Id &&
                                                x.fk_VoucherDetail.VoucherId != vdr.fk_VoucherDetail.VoucherId)
                                        .Sum(x => (decimal?)x.Amount_MNC);
                                
                                if (vdr.CurTypeId == Helper.ConstCurTypeId)
                                {
                                    /*local currency*/
                                    if (!childsum.HasValue) childsum = 0;
                                    if ((parentAmt > 0 && (parentAmt + vdr.Amount_MNC + childsum) < 0) ||
                                        (parentAmt < 0 && (parentAmt + vdr.Amount_MNC + childsum) > 0))
                                    {
                                        throw new BusinessException(ErrorCode.VCH109,
                                            $"Core:Ref No:{vdr.ReferenceNo} or ParentRefId:{vdr.RefId.GetValueOrDefault(0).ToString()}, Ref balance Amt:{(parentAmt + childsum).ToString()}");
                                    }
                                }
                                else
                                {   /*foreign currency*/
                                    vdr.Amount_FX = Math.Round(-1 * (vdr.CurRate - parentVDR.CurRate) * vdr.Amount, 2);

                                    if (!childsum.HasValue) childsum = 0;
                                    if ((parentAmt > 0 && (parentAmt + vdr.Amount_MNC + childsum) < 0) ||
                                        (parentAmt < 0 && (parentAmt + vdr.Amount_MNC + childsum) > 0))
                                    {
                                        throw new BusinessException(ErrorCode.VCH109,
                                            $"Core:Ref No:{vdr.ReferenceNo} or ParentRefId:{vdr.RefId.GetValueOrDefault(0).ToString()}, Ref balance Amt:{(parentAmt + childsum).ToString()}");
                                    }
                                }
                            }
                        }
                    }

                    #region Currency Conversion
                    if (vdr.VDRTypeId != 1014 && vdr.IsCCRequired)
                    {
                        try
                        {
                            vdr.CurTypeId = vdr.CurTypeId ?? vdr.fk_VoucherDetail.CurTypeId;
                            vdr.CurRate = vdr.CurRate <= 0 ? vdr.fk_VoucherDetail.CurRate : vdr.CurRate;
                            vdr.Amount_MNC = Math.Round(vdr.CurRate * vdr.Amount,2);
                        }
                        catch
                        {
                            //nothing to do 
                        }
                    }
                    #endregion
                    break;

                case "SpareLog":
                    new SpareLogCoreLogic().Bind(this).Execute(entry);
                    break;

                case "SpareLogExtraInfo":
                    var sle = entry.Entity as SpareLogExtraInfo;
                    var sledbset = this.Set<SpareLogExtraInfo>();
                    var purchasetypes = new List<long> { 22 /*Consume*/, 61 /*Material Purchase Bill*/, 23  /* Material MRN */, 62    /* Material Bill[MRN Settlement] */ };
                    if (sle == null || sle.ObjectState == ObjectState.Unchanged || !purchasetypes.Contains(sle.VoucherTypeId.GetValueOrDefault())) break;
                    var condition = GetApiConfig<int>("FleetVendorRefNoDuplicateCheck");/*0:Don't Validate Duplicate, 1:Validate Duplicate if Not Empty, 2:Should Not Empty and Validate Duplicate*/
                    var vedorRefNo = sle.VendorReferenceNo?.Trim(' ') ?? "";
                    if (sle?.fk_Voucher != null)
                    {
                        sle.fk_Voucher.UserRemark = sle.Remark;
                    }
                    if (condition == 0 || (condition == 1 && string.IsNullOrWhiteSpace(vedorRefNo))) break;
                    var fy = this.FinancialYears.Where(
                    x =>
                        x.OpeningDate <= sle.DocDate && x.ClosingDate >= sle.DocDate &&
                        x.IsActive).Select(x => new { x.Id, x.OpeningDate, x.ClosingDate }).FirstOrDefault();
                    if (fy != null)
                    {
                        if (sledbset.Any(x => x.VendorReferenceNo == vedorRefNo && x.Id != sle.Id && x.CrAccountId == sle.CrAccountId && fy.OpeningDate <= x.DocDate && fy.ClosingDate >= x.DocDate))
                        {
                            throw new BusinessException(ErrorCode.GLB104, "Trasaction already Exists with same Vendor Bill No for Specified Vendor");
                        }
                    }
                    break;

                case "TyreLogExtraInfo":
                    var tyr = entry.Entity as TyreLogExtraInfo;
                    var tyrset = this.Set<TyreLogExtraInfo>();
                    var tyrepurchasetypes = new List<long> { 27 /*Tyre Purchased*/};
                    if (tyr == null || tyr.ObjectState == ObjectState.Unchanged || !tyrepurchasetypes.Contains(tyr.VoucherTypeId)) break;
                    var tycondition = GetApiConfig<int>("FleetVendorRefNoDuplicateCheck");/*0:Don't Validate Duplicate, 1:Validate Duplicate if Not Empty, 2:Should Not Empty and Validate Duplicate*/
                    var tyvedorRefNo = tyr.VendorReferenceNo?.Trim(' ') ?? "";
                    if (tycondition == 0 || (tycondition == 1 && string.IsNullOrWhiteSpace(tyvedorRefNo))) break;
                    var tfy = this.FinancialYears.Where(
                    x =>
                        x.OpeningDate <= tyr.VoucherDate && x.ClosingDate >= tyr.VoucherDate &&
                        x.IsActive).Select(x => new { x.Id, x.OpeningDate, x.ClosingDate }).FirstOrDefault();
                    if (tyrset.Any(x => x.VendorReferenceNo == tyvedorRefNo && x.Id != tyr.Id && x.CrAccountId == tyr.CrAccountId && tfy.OpeningDate <= x.VoucherDate && tfy.ClosingDate >= x.VoucherDate))
                    {
                        throw new BusinessException(ErrorCode.GLB104, "Trasaction already Exists with same Vendor Bill No for Specified Vendor");
                    }

                    if (tyr?.fk_Voucher != null)
                    {
                        tyr.fk_Voucher.UserRemark = tyr.Remark;
                    }
                    break;

                case "BatteryLogExtraInfo":
                    var btry = entry.Entity as BatteryLogExtraInfo;
                    var btryset = this.Set<BatteryLogExtraInfo>();
                    var btpurchasetypes = new List<long> { 43 /*Battery Purchase*/};
                    if (btry == null || btry.ObjectState == ObjectState.Unchanged || !btpurchasetypes.Contains(btry.VoucherTypeId)) break;
                    var btcondition = GetApiConfig<int>("FleetVendorRefNoDuplicateCheck");/*0:Don't Validate Duplicate, 1:Validate Duplicate if Not Empty, 2:Should Not Empty and Validate Duplicate*/
                    var btvedorRefNo = btry.VendorReferenceNo?.Trim(' ') ?? "";
                    if (btcondition == 0 || (btcondition == 1 && string.IsNullOrWhiteSpace(btvedorRefNo))) break;
                    var bfy = this.FinancialYears.Where(
                   x =>
                       x.OpeningDate <= btry.DocDate && x.ClosingDate >= btry.DocDate &&
                       x.IsActive).Select(x => new { x.Id, x.OpeningDate, x.ClosingDate }).FirstOrDefault();
                    if (btryset.Any(x => x.VendorReferenceNo == btvedorRefNo && x.Id != btry.Id && x.CrAccountId == btry.CrAccountId && bfy.OpeningDate <= x.DocDate && bfy.ClosingDate >= x.DocDate))
                    {
                        throw new BusinessException(ErrorCode.GLB104, "Trasaction already Exists with same Vendor Bill No for Specified Vendor");
                    }
                    if (btry?.fk_Voucher != null)
                    {
                        btry.fk_Voucher.UserRemark = btry.Remark;
                    }
                    break;
                
                case "CNExtraInfo":
                    new CNExtraInfoCoreLogic().Bind(this).Execute(entry);
                    break;

                case "VehiclePreventiveLog":
                    new VehiclePreventiveLogCoreLogic().Bind(this).Execute(entry);
                    break;

                case "VTSStatusLog":
                    new VTSStatusLogCoreLogic().Bind(this).Execute(entry);
                    break;
                
                case "TripAdvanceLog":

                    var adv = entry.Entity as TripAdvanceLog;

                    if (adv != null && adv.ObjectState != ObjectState.Added)
                    {
                        VehicleTripSettlement setl = null;
                        if (adv.SettlementId.HasValue)
                        {
                            try
                            {
                                setl = ChangeTracker.Entries<VehicleTripSettlement>().Select(x => x.Entity)
                                    .FirstOrDefault(x => x.Id == adv.SettlementId);
                            }
                            catch (Exception)
                            {
                                //Ignore
                            }
                        }
                        var advo = entry.OriginalValues;
                        if (adv == null) break;
                        if (adv.ObjectState == ObjectState.Unchanged) break;
                        if (adv?.SettlementId != null && adv.ObjectState == ObjectState.Deleted)
                        {
                            throw new BusinessException(ErrorCode.TADV105,
                                "Core:Cannot Delete Reference No:" + adv.ReferenceNo);
                        }
                        //if (advo["SettlementId"] != null && adv.SettlementId.HasValue && (setl!=null&&
                        //    setl.Id != adv?.SettlementId) &&
                        //    adv.ObjectState == ObjectState.Modified)
                        //{
                        //    throw new BusinessException(ErrorCode.TADV105,
                        //        "Core:Cannot Modify Reference No:" + adv.ReferenceNo);
                        //}
                        if (adv.ObjectState == ObjectState.Modified && advo["SettlementId"] != null && adv.SettlementId.GetValueOrDefault() > 0 && setl == null)
                        {
                            throw new BusinessException(ErrorCode.TADV105,
                                "Core:Cannot Modify Reference No:" + adv.ReferenceNo);
                        }
                    }
                    adv.FuelExpanses?.RemoveAll(x => x.ObjectState == ObjectState.Deleted);

                    if (adv.VehicleId > 0 && this.Set<VehicleMaster>().Where(x => x.Id == adv.VehicleId && (x.SoldDate != null || x.IsDeactive)).Any())
                    {
                        //Check if Vehicle is not sold and vehicle is not deactivated
                        throw new BusinessException(ErrorCode.GLB106, "Selected Vehicle In Advance is either Deactivated or has been sold.");
                    }
                    if (adv.HireVehicleId > 0 && this.Set<HireVehicle>().Where(x => x.Id == adv.HireVehicleId && (x.IsBlackListed || x.fk_HireParty.IsDefaulter)).Any())
                    {
                        //Check if Vehicle is not deactivated or blacklisted
                        throw new BusinessException(ErrorCode.GLB106, "Selected Hire Vehicle In Advance is either BlackListed or Hire Party is BlackListed.");
                    }
                    if (adv.HireVehicleId > 0 && adv.DebitAccountId > 0 && this.Set<Ledger>().Any(x => x.Id == adv.DebitAccountId && x.IsDefaulter))
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Selected Debit Account in Advance is BlackListed.");
                    }
                    new TripAdvanceCoreLogic().Bind(this).Execute(entry);
                    break;

                case "VehicleMovementLog":
                    DbPropertyValues tripLogDb = null;
                    int TripLogJobcardChainingStatus = this.GetApiConfig<int>("TripLogJobcardChainingStatus");

                    int TripLogJobcardOverlapping = this.GetApiConfig<int>("TripLogJobcardOverlapping");

                    var tripLog = entry.Entity as VehicleMovementLog;
                    if (tripLog.VehicleId > 0 && this.Set<VehicleMaster>().Where(x => x.Id == tripLog.VehicleId && (x.SoldDate != null || x.IsDeactive)).Any())
                    {
                        //Check if Vehicle is not sold and vehicle is not deactivated
                        throw new BusinessException(ErrorCode.GLB106, $"Selected Vehicle In Trip is either Deactivated or has been sold.Hind vehicleId {tripLog.VehicleId}");
                    }
                    if (tripLog.HireVehicleId > 0 && (this.Set<HireVehicle>().Where(x => x.Id == tripLog.HireVehicleId && (x.IsBlackListed || x.fk_HireParty.IsDefaulter)).Any()))
                    {
                        //Check if Vehicle is not deactivated or blacklisted
                        throw new BusinessException(ErrorCode.GLB106, "Selected Hire Vehicle In Trip is either BlackListed or Hire Party is BlackListed.");
                    }
                    if (tripLog.HVPId > 0 && this.Set<Ledger>().Any(x => x.Id == tripLog.HVPId && x.IsDefaulter))
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Selected Hire Party In Trip is BlackListed.");
                    }
                    List<long> triptypes = new List<long>();
                    switch (TripLogJobcardChainingStatus)
                    {
                        case 0:
                            triptypes = new List<long>() { 1158, 1159 };
                            if (tripLog.VehicleId != null && tripLog.TripTypeId == 1160)
                            {
                                triptypes.Add(1160);
                            }
                            break;

                        case 1:
                            triptypes.Add(tripLog.TripTypeId.Value);
                            break;
                    }
                    if (tripLog?.Id > 0)
                    {
                        tripLogDb = entry.OriginalValues;
                    }
                    if (tripLog?.ObjectState == ObjectState.Deleted)
                    {
                        var setid = (long?)tripLogDb?["SettlementId"];
                        if (setid.HasValue && setid.Value > 0)
                        {
                            throw new BusinessException(ErrorCode.TAL100, tripLog.TriplogNo);
                        }
                        else if (this.Set<CNExtraInfo>().Where(x => x.TripLogId == tripLog.Id).Any()) {
                            throw new BusinessException(ErrorCode.POD100, tripLog.TriplogNo);
                        }
                    }
                    try
                    {
                        var triplogrepo = this.Set<VehicleMovementLog>();
                        if (tripLog != null && (tripLog.TripTypeId == 1158||(tripLog.TripTypeId == 1160&&tripLog.VehicleId!=null) || tripLog.TripNatureId == 1076))
                        {
                            var unloaddate = tripLog.UnloadingDate ?? DateTime.Now;
                            if (tripLog.ObjectState != ObjectState.Deleted &&
                                tripLog.ObjectState != ObjectState.Unchanged)
                            {
                                bool verifydate = true;
                                if (tripLog.ObjectState == ObjectState.Modified)
                                {
                                    try
                                    {
                                        //If Triplog is modified check if date has to check for overlap
                                        //This privious code in changeset no 2120 was working fine to check date overlap but the code flow in that was terminated in below if statement hence it was not reaching at stationary modification check code.so now i need to change this code as now it is.
                                        if (((tripLog.UnloadingDate.GetValueOrDefault() == default(DateTime) &&
                                              entry.OriginalValues["UnloadingDate"] == null) ||
                                             tripLog.UnloadingDate.Equals(entry.OriginalValues["UnloadingDate"])) &&
                                            tripLog.TripStartDate.Equals(entry.OriginalValues["TripStartDate"]))
                                        {
                                            //return;
                                            verifydate = false;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }

                                if (verifydate && TripLogJobcardOverlapping == 0)
                                {
                                    var result = (from t in triplogrepo
                                                  where
                                                      ((t.TripStartDate >= tripLog.TripStartDate && t.TripStartDate <= unloaddate)
                                                       ||
                                                       ((t.UnloadingDate == null && DateTime.Now >= tripLog.TripStartDate) &&
                                                        (t.UnloadingDate == null && DateTime.Now <= unloaddate))
                                                       ||
                                                       ((t.UnloadingDate != null && t.UnloadingDate >= tripLog.TripStartDate) &&
                                                        (t.UnloadingDate != null && t.UnloadingDate <= unloaddate))
                                                       ||
                                                       (t.TripStartDate <= tripLog.TripStartDate &&
                                                        ((t.UnloadingDate != null && t.UnloadingDate >= unloaddate) ||
                                                         (t.UnloadingDate == null && DateTime.Now >= unloaddate)))
                                                      ) && t.Id != tripLog.Id && t.VehicleId == tripLog.VehicleId &&
                                                      t.HireVehicleId == tripLog.HireVehicleId &&
                                                      ((t.TripTypeId == 1158 || (t.TripTypeId == 1160 && t.VehicleId != null) || t.TripTypeId == 1453) || t.TripNatureId == 1076) &&
                                                      triptypes.Contains(t.TripTypeId.Value)
                                                  select t.TriplogNo).FirstOrDefault();
                                    if (tripLog.ObjectState == ObjectState.Modified &&
                                        !string.IsNullOrWhiteSpace(result) &&
                                        (!entry.OriginalValues["TripStartDate"].Equals(tripLog.TripStartDate) ||
                                         (entry.OriginalValues["UnloadingDate"] == null &&
                                          tripLog.UnloadingDate.HasValue) ||
                                         !entry.OriginalValues["UnloadingDate"].Equals(tripLog.UnloadingDate)))
                                    {
                                        throw new BusinessException(ErrorCode.GLB106,
                                            $"Cannot Update this TripLog as trip/jobsheet already exist between Trip Start Date :{tripLog.TripStartDate:F} and UnloadDate:{unloaddate:F} with Number with number :{result}");
                                    }

                                    if (!string.IsNullOrWhiteSpace(result))
                                        throw new BusinessException(ErrorCode.GLB106,
                                            $"Cannot Update/Insert this TripLog as trip/jobsheet already exist between Trip Start Date :{tripLog.TripStartDate:F} and UnloadDate:{unloaddate:F} with Number with number :{result}");
                                }
                            }
                        }

                        switch (tripLog.ObjectState)
                        {
                            case ObjectState.Added:
                            case ObjectState.Modified:
                                if (tripLog.TripTypeId == 1664)
                                {
                                    //if (tripLog.UnloadingDate == null)
                                    //{
                                        //if (tripLog.LoadTypeId.GetValueOrDefault(0) <= 0)
                                        //{
                                        //    throw new BusinessException(ErrorCode.GLB106, "LoadType Is Required For Trip Schedule");
                                        //}
                                        if (this.GetApiConfig<int>("AvoidScheduleOnOpenTrip") == 1)
                                        {
                                            var nonloadedtripnatures = new List<long?> { 1520, 1076 };
                                            var triptypesf = new List<long?> { 1158/*TripLog*/, 1453/*Dispatch*/, 1664/*Schedule*/ };
                                            var tls = triplogrepo.Where(x => (x.VehicleId == tripLog.VehicleId) && !nonloadedtripnatures.Contains(x.TripNatureId) && ((x.LoadingDate == null && x.TripTypeId == 1158 && x.UnloadingDate == null) || (x.UnloadingDate == null && x.TripTypeId == 1664)) && (x.Id != tripLog.Id)).ToList();
                                            if (tls.Any())
                                            {
                                                var tldata = tls.Select(x => x.TriplogNo).ToList().JoinStrings(",");
                                                throw new BusinessException(ErrorCode.GLB106, "Source Schedule:Either Trip is open without loading date or tripschedule already exists tripNos: " + tldata);
                                            }
                                        }
                                   // }
                                }
                                //else if (tripLog.TripTypeId == 1158 && tripLog.TripNatureId==1075)
                                //{
                                //    if (tripLog.UnloadingDate == null)
                                //    {
                                //        if (this.GetApiConfig<int>("AvoidScheduleOnOpenTrip") == 1)
                                //        {
                                //            var nonloadedtripnatures = new List<long?> { 1520, 1076 };
                                //            var triptypesf = new List<long?> { 1158/*TripLog*/, 1453/*Dispatch*/, 1664/*Schedule*/ };
                                //            var tls = triplogrepo.Where(x => (x.VehicleId == tripLog.VehicleId) && !nonloadedtripnatures.Contains(x.TripNatureId) && ((x.LoadingDate == null && x.TripTypeId == 1158 && x.UnloadingDate == null) || (x.UnloadingDate == null && x.TripTypeId == 1664)) && (x.Id != tripLog.Id)).ToList();
                                //            if (tls.Any())
                                //            {
                                //                var tldata = tls.Select(x => x.TriplogNo).ToList().JoinStrings(",");
                                //                throw new BusinessException(ErrorCode.GLB106, "Source Triplog:Either Trip is open without loading date or tripschedule already exists tripNos: " + tldata);
                                //            }
                                //        }
                                //    }
                                //}

                                if (tripLog.ObjectState == ObjectState.Modified && (tripLog.TripTypeId == 1158 || (tripLog.TripTypeId == 1160 && tripLog.VehicleId != null) || tripLog.TripTypeId == 1453 || tripLog.TripTypeId == 1664))
                                {
                                    try
                                    {
                                        var routeid = entry.Property("RouteId");

                                        if (!routeid.CurrentValue.Equals(routeid.OriginalValue))
                                        {
                                            this.Database.ExecuteSqlCommand($"DELETE [dbo].[tPickDroplog] WHERE RouteId={routeid.OriginalValue} AND TripLogId={tripLog.Id} AND TripLogId>0");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //Ignore
                                    }
                                }
                                var previousTripLog =
                                    triplogrepo
                                        .OrderByDescending(x => x.TripStartDate)
                                        .ThenByDescending(x => x.Id)
                                        .Include(x => x.fk_NextLog)
                                        .FirstOrDefault(
                                            x =>
                                                x.VehicleId == tripLog.VehicleId && x.HireVehicleId == tripLog.HireVehicleId &&
                                                x.TripStartDate <= tripLog.TripStartDate && x.Id != tripLog.Id && (x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null) || x.TripTypeId == 1453 || (x.TripNatureId == 1076 && x.TripTypeId == 1159)));

                                if (previousTripLog != null)
                                {
                                    var pnexttriplog = previousTripLog.fk_NextLog;
                                    if (pnexttriplog != null && pnexttriplog.Id != tripLog.Id)
                                    {
                                        pnexttriplog.PreviousLogId = tripLog.Id;
                                        pnexttriplog.fk_PreviousLog = tripLog;
                                        tripLog.fk_NextLog = pnexttriplog;
                                        tripLog.NextLogId = pnexttriplog.Id;
                                        pnexttriplog.ObjectState = ObjectState.Modified;
                                        triplogrepo.AddOrUpdate(pnexttriplog);
                                    }
                                    tripLog.PreviousLogId = previousTripLog.Id;
                                    tripLog.fk_PreviousLog = previousTripLog;
                                    previousTripLog.NextLogId = tripLog.Id;
                                    previousTripLog.fk_NextLog = tripLog;
                                    previousTripLog.ObjectState = ObjectState.Modified;
                                    triplogrepo.AddOrUpdate(previousTripLog);
                                }
                                var config = GetApiConfig<int>("AllowOnlyParentChildFromPlace");
                                if (config == 1 && tripLog.RouteId > 0 && tripLog.VehicleId.GetValueOrDefault() > 0 && ((tripLog.TripTypeId == 1664 && tripLog.UnloadingDate == null/*Only Open Schedule*/) || tripLog.TripTypeId ==1158/*TripLog*/|| tripLog.TripTypeId == 1453/*LocalDispatch*/))
                                {
                                    var ptl =
                                        triplogrepo
                                            .OrderByDescending(x => x.TripStartDate)
                                            .ThenByDescending(x => x.Id)
                                            .Include(x => x.fk_NextLog)
                                            .FirstOrDefault(
                                                x =>
                                                    x.VehicleId == tripLog.VehicleId &&
                                                    x.TripStartDate <= tripLog.TripStartDate && x.Id != tripLog.Id && (x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null) || x.TripTypeId == 1453));
                                    if (ptl != null)
                                    {
                                        /*changes 2025-10-28 by sanjay kushwaha*/
                                        var route_cities = Set<RouteMaster>().Where(x => x.Id == tripLog.RouteId).Select(x => new { CityId=x.FromPlaceId, FromParentId = x.fk_FromPlace.ParentCityId }).FirstOrDefault();
                                       
                                        var cur_waypoints = Set<VehicleMovementLogPickupDrop>().Where(x => x.TriplogId == tripLog.Id).Select(x => new { x.Order, x.CityId, FromParentId = x.fk_City.ParentCityId }).ToList();

                                        
                                        var curcityid =
                                            cur_waypoints?.OrderBy(y => y.Order).FirstOrDefault()
                                                ?.CityId ?? 0;

                                        var curParentid =
                                            cur_waypoints?.OrderBy(y => y.Order).FirstOrDefault()
                                                ?.FromParentId ?? curcityid;

                                        if (curcityid == 0 && route_cities != null)
                                        {
                                            curcityid = route_cities.CityId;

                                            curParentid = route_cities.FromParentId ?? 0;
                                        }

                                        var pre_waypoints = Set<VehicleMovementLogPickupDrop>().Where(x => x.TriplogId == ptl.Id).Select(x => new { x.Order, x.CityId, ToParentId = x.fk_City.ParentCityId }).ToList();

                                        /*changes 2023-11-23 by sanjay kushwaha*/
                                        var precityid =
                                            pre_waypoints?.OrderByDescending(y => y.Order).FirstOrDefault()
                                                ?.CityId ?? 0;
                                        var pre_parentid =
                                            pre_waypoints?.OrderByDescending(y=>y.Order).FirstOrDefault()
                                                ?.ToParentId ?? precityid;
                                      
                                        /*
                                         * A is Parent, B is Previous City, C is Current City
                                         * A<=>A Both have same parent
                                         * A<=>B Previous is Parent and Current is Child
                                         * A<=>C Previous is Parent and Current is Child
                                         * B<=>C Both have same parent
                                         * B<=>B Previous and Current are same
                                         * C<=>C Previous and Current are same
                                         */
                                        var iscityok = precityid == curcityid /*B<=>B or C<=>C*/
                                                       || pre_parentid ==
                                                       curParentid /*A<=>A,A<=>B,A<=>C,B<=>C,C<=>C*/;
                                        if (!iscityok)
                                        {
                                            throw new BusinessException(ErrorCode.GLB106, $"Selected Trip Route is not valid as it does not qualify Parent Child city Rule. precityid({precityid}) == curcityid({curcityid}) || pre_parentid({ pre_parentid }) == curParentid({ curParentid}) for CurTripId({tripLog.TriplogNo}) and PreviousTripId({ptl.TriplogNo})");
                                        }
                                    }
                                }
                                break;

                            case ObjectState.Deleted:
                                VehicleMovementLog previoustrip1 = tripLog.fk_PreviousLog ??
                                                                   triplogrepo.OrderByDescending(
                                                                       x => x.TripStartDate)
                                                                       .ThenByDescending(x => x.Id)
                                                                       .FirstOrDefault(
                                                                           x => x.NextLogId == tripLog.Id && x.Id != tripLog.Id);
                                VehicleMovementLog nextTLog = tripLog.fk_NextLog ??
                                                              triplogrepo.OrderByDescending(x => x.TripStartDate)
                                    .ThenByDescending(x => x.Id)
                                    .FirstOrDefault(x => x.PreviousLogId == tripLog.Id);

                                if (previoustrip1 != null)
                                {
                                    //if (tripLog.fk_NextLog == null)
                                    //{
                                    //    tripLog.fk_NextLog =
                                    //        triplogrepo.OrderByDescending(x => x.TripStartDate)
                                    //            .ThenByDescending(x => x.Id)
                                    //            .FirstOrDefault(x => x.Id == tripLog.NextLogId);
                                    //}
                                    //if (this.Entry(previoustrip1).State == EntityState.Detached)
                                    //{
                                    //    triplogrepo.Attach(previoustrip1);
                                    //}
                                    previoustrip1.NextLogId = nextTLog?.Id;
                                    previoustrip1.fk_NextLog = nextTLog;
                                    previoustrip1.ObjectState = ObjectState.Modified;
                                    if (previoustrip1.fk_NextLog != null)
                                    {
                                        //var pventry = this.Entry(previoustrip1.fk_NextLog);
                                        //if (pventry.State == EntityState.Detached)
                                        //{
                                        //    triplogrepo.Attach(previoustrip1.fk_NextLog);
                                        //}
                                        previoustrip1.fk_NextLog.ObjectState = ObjectState.Modified;
                                        previoustrip1.fk_NextLog.fk_PreviousLog = previoustrip1;
                                        previoustrip1.fk_NextLog.PreviousLogId = previoustrip1.Id;
                                    }
                                    tripLog.PreviousLogId = null;
                                    tripLog.fk_PreviousLog = null;
                                    tripLog.fk_NextLog = null;
                                    tripLog.NextLogId = null;
                                }
                                else//if this is a first Trip
                                {
                                    if (nextTLog != null)
                                    {
                                        nextTLog.ObjectState = ObjectState.Modified;
                                        nextTLog.fk_PreviousLog = null;
                                        nextTLog.PreviousLogId = null;
                                    }
                                }
                                //TODO: Commented Temporary if(entry.State!=EntityState.Deleted) entry.State=EntityState.Deleted;

                                if (tripLog != null && tripLog.TripTypeId==1159)
                                {
                                    DeleteApprovalLogs("PartRequestData1020", tripLog.Id);
                                }
                                break;
                        }
                        //}
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetBaseException().Message.Equals("Sequence contains more than one element"))
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Integrity Check Raised an Error.");
                        }
                        throw;
                    }
                    try
                    {
                        new CalTyreMillageVMLCoreLogic().Bind(this).Execute(entry);
                    }
                    catch (BusinessException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                    }

                    break;

                case "TripExpenseLog":

                    var tripexpense = entry.Entity as TripExpenseLog;
                    if (tripexpense?.ObjectState != ObjectState.Added)
                    {
                        var orgdb = entry.OriginalValues;
                        if (tripexpense.ObjectState == ObjectState.Unchanged) break;
                        if (tripexpense.SettlementId > 0 && tripexpense.ObjectState == ObjectState.Deleted)
                        {
                            throw new BusinessException(ErrorCode.TEXP100,
                               $"Core:Delete Failed - Expense (Id:{tripexpense.Id}) has been settled.");
                        }
                        //if (tripexpense.Id > 0)
                        //{
                        //    var _oldexp = this.Set<TripExpenseLog>()
                        //       .Where(x => x.Id == tripexpense.Id).FirstOrDefault();
                        //    if (_oldexp.SettlementId != null)
                        //    {
                        //        if (_oldexp.SettlementId > 0 && tripexpense.SettlementId.HasValue &&
                        //            _oldexp.SettlementId != tripexpense.SettlementId &&
                        //            tripexpense.ObjectState == ObjectState.Modified)
                        //        {
                        //            var tsno = this.Set<VehicleTripSettlement>()
                        //            .Where(x => x.Id == _oldexp.SettlementId)
                        //            .Select(y => y.TripSheetNo).FirstOrDefault();

                        //            throw new BusinessException(ErrorCode.TEXP100,
                        //                $"Core:Modify Failed - Expense (Id:{tripexpense.Id})={tripexpense.fk_ExpenseType.Name} has been settled with TSNo:{tsno}.");
                        //        }
                        //    }
                        //}
                    }
                    if (GetApiConfig<int>("SettlementDraftEnabled") == 1) {
                        if (tripexpense.SettlementId != null)
                        {
                            switch (tripexpense.ObjectState)
                            {
                                case ObjectState.Added:
                                case ObjectState.Modified:
                                    //var draft = this.Set<VehicleTripSettlement>().FirstOrDefault(y=>y.)
                                    break;
                                case ObjectState.Deleted:
                                    break;
                            }
                        }
                    }
                    break;

                case "BatteryLog":
                    var batteryLog = entry.Entity as BatteryLog;
                    if (batteryLog?.ObjectState == ObjectState.Deleted)
                    {
                        var tyredbset = Set<BatteryLog>();
                        if (tyredbset.Any(x => x.NextLogId == batteryLog.Id))
                        {
                            foreach (var source in tyredbset.Where(x => x.NextLogId == batteryLog.Id).ToList())
                            {
                                source.NextLogId = null;
                                source.fk_NextLog = null;
                                source.ObjectState = ObjectState.Modified;
                                tyredbset.Attach(source);
                            }
                        }
                        batteryLog.PreviousLogId = null;
                        batteryLog.fk_PreviousLog = null;
                    }
                    try
                    {
                        new BatteryLogCoreLogic().Bind(this).Execute(entry);
                    }
                    catch (BusinessException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        //ex.ToExceptionless().AddObject(entry).Submit();
                    }
                    break;

                case "TyreLog":
                    var tyrelog = entry.Entity as TyreLog;
                    if (tyrelog?.ObjectState == ObjectState.Deleted)
                    {
                        var tyredbset = Set<TyreLog>();
                        if (tyredbset.Any(x => x.NextLogId == tyrelog.Id))
                        {
                            foreach (var source in tyredbset.Where(x => x.NextLogId == tyrelog.Id).ToList())
                            {
                                source.NextLogId = null;
                                source.fk_NextLog = null;
                                source.ObjectState = ObjectState.Modified;
                                tyredbset.Attach(source);
                            }
                        }
                        
                        tyrelog.PreviousLogId = null;
                        tyrelog.fk_PreviousLog = null;
                    }
                    try
                    {
                        /*new CalTyreMillageTyreLogCoreLogic().Bind(this).Execute(entry);*/
                    }
                    catch (BusinessException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                    }
                    break;

                case "Ledger":
                    var ledger = entry.Entity as Ledger;
                    #region bill to bill logic
                    if (ledger.AccountRoleId== 1020 || ledger.AccountRoleId == 1085 || ledger.AccountRoleId == 1998) {
                        ledger.ReferenceFlag = true;
                    }
                    #endregion

                    var opflag = this.GetApiConfig<int>("InitialOpeningBalaceRestirction") == 1;
                    if (ledger?.EffectiveDate != null && (ledger.ObjectState == ObjectState.Modified || ledger.ObjectState == ObjectState.Modified))
                    {
                        if (this.Set<VoucherDetail>()
                            .Any(x => x.AccountId == ledger.Id && x.Voucher.FinancialYearId!=null && x.Voucher.VoucherDate < ledger.EffectiveDate && x.Voucher.VoucherTypeId != 19) && opflag)
                        {
                            throw new BusinessException(ErrorCode.GLB106, $"Invalid Effective Date.\nIn System There is a Transaction releated to {ledger.AccountName} before Effective Date {ledger.EffectiveDate.GetValueOrDefault().ToString("dd-MMM-yyyy")}");
                        }
                    }

                    #region Add Ledger Role Logic

                    var repo = this.Set<LedgerRole>();
                    if (ledger?.ObjectState == ObjectState.Modified)
                    {
                        //Check if Original and Current Account RoleId changed
                        if (!Equals(entry.Property("AccountRoleId").OriginalValue, ledger.AccountRoleId))
                        {
                            //If Original or Current value is Null, Modify the default Role in Ledger Role
                            var existingLedgerRole =
                                repo.FirstOrDefault(x => x.IsDefault && x.LedgerId == ledger.Id);
                            if (existingLedgerRole != null && ledger.AccountRoleId.GetValueOrDefault() > 0)
                            {
                                existingLedgerRole.RoleId = ledger.AccountRoleId.GetValueOrDefault();
                                existingLedgerRole.ObjectState = ObjectState.Modified;
                                repo.AddOrUpdate(existingLedgerRole);
                            }
                            else if (existingLedgerRole != null && ledger.AccountRoleId.GetValueOrDefault() <= 0)
                            {
                                existingLedgerRole.ObjectState = ObjectState.Deleted;
                                repo.Remove(existingLedgerRole);
                            }
                            else if (ledger.AccountRoleId.GetValueOrDefault(0) > 0)
                            {
                                var ledgerrole = new LedgerRole()
                                {
                                    LedgerId = ledger.Id,
                                    RoleId = ledger.AccountRoleId.GetValueOrDefault(),
                                    IsDefault = true,
                                    ObjectState = ObjectState.Added,
                                    fk_Ledger = ledger
                                };
                                repo.Add(ledgerrole);
                            }
                        }
                    }
                    else if (ledger?.ObjectState == ObjectState.Added && ledger.AccountRoleId.GetValueOrDefault() > 0)
                    {
                        var ledgerrole = new LedgerRole()
                        {
                            LedgerId = ledger.Id,
                            RoleId = ledger.AccountRoleId.GetValueOrDefault(),
                            IsDefault = true,
                            ObjectState = ObjectState.Added,
                            fk_Ledger = ledger
                        };
                        repo.Add(ledgerrole);
                    }
                    #endregion Add Ledger Role Logic
                    break;
                case "StationeryBook":
                    var book = entry.Entity as StationeryBook;
                    if (book != null)
                    {
                        if (book.ObjectState == ObjectState.Added || book.ObjectState == ObjectState.Modified)
                        {//TODO:Re write below Logic as it seems to wrong. I have faced some issue in it and at that time i was not having time so left it as it was.
                         //var y1 = book.StartingNumber + book.NoOfPages-1;//New To Number[B]

                            //var existing =
                            //    this.Set<StationeryBook>()
                            //        .Where(
                            //            x => x.Id != book.Id &&
                            //                 x.Prefix == book.Prefix && x.TypeId == book.TypeId &&
                            //                 ((book.StartingNumber >= x.StartingNumber && book.StartingNumber <= x.StartingNumber + x.NoOfPages - 1) ||
                            //                 (y1 >= x.StartingNumber && y1 <= x.StartingNumber + x.NoOfPages - 1) ||
                            //                 (x.StartingNumber>=book.StartingNumber&&x.StartingNumber<=y1)||
                            //                 (x.StartingNumber+x.NoOfPages - 1 >= book.StartingNumber && x.StartingNumber + x.NoOfPages - 1 <= y1)))
                            //        .Select(x => new
                            //        {
                            //            x.Name,
                            //            OfficeName = x.fk_Office != null ? x.fk_Office.OfficeName : null
                            //        }).FirstOrDefault();
                            var existing1 = this.Database.SqlQuery<string>($"EXEC Proc_TRNS_STNRY_DuplicateCheck  {book.Id},{book.TypeId},{book.StartingNumber},{book.NoOfPages},'{book.Prefix}'").FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(existing1))
                                throw new BusinessException(ErrorCode.GLB106,
                                    $"Core:Some of the pages are matching with existing book '{existing1.Split('^')[0]}'{(string.IsNullOrWhiteSpace(existing1.Split('^')[1]) ? "" : (" which is mapped to office " + existing1.Split('^')[1]))}.");
                        }
                        if (book.ObjectState == ObjectState.Deleted &&
                            this.Set<StationeryBook>()
                                .Any(
                                    x =>
                                        x.Id == book.Id &&
                                        (x.IsUsed || x.IsLocked ||
                                         !(x.PreviousUsedPage == null || x.PreviousUsedPage.Trim() == string.Empty))))
                            throw new BusinessException(ErrorCode.GLB106, "Book has been Locked or Used.");
                        //string.IsNullOrWhiteSpace(x.PreviousUsedPage)
                    }
                    break;

                case "DriverVehicleMapping":
                    var mapping = entry.Entity as DriverVehicleMapping;
                    if (mapping != null && mapping.ObjectState != ObjectState.Unchanged && (mapping.DriverStatusId == 1657/*BlackList On*/|| mapping.DriverStatusId == 1658/*BlackList Off*/))
                    {
                        var driverrepo = this.Set<DriverMaster>();
                        var driverrec = driverrepo.Include(x => x.fk_Ledger).FirstOrDefault(x => x.Id == mapping.DriverId);
                        switch (mapping.ObjectState)
                        {
                            case ObjectState.Added:
                            case ObjectState.Modified:

                                if (driverrec != null)
                                {
                                    driverrec.Status = mapping.DriverStatusId == 1657
                                        ? MasterStatus.BlackListed
                                        : MasterStatus.Active;
                                    driverrec.ObjectState = ObjectState.Modified;
                                    driverrec.fk_Ledger.IsDefaulter = driverrec.Status == MasterStatus.BlackListed;
                                    driverrec.fk_Ledger.ObjectState = ObjectState.Modified;
                                }
                                break;

                            case ObjectState.Deleted:

                                if (driverrec != null)
                                {
                                    driverrec.Status = mapping.DriverStatusId == 1658
                                        ? MasterStatus.BlackListed :
                                        MasterStatus.Active;
                                    driverrec.fk_Ledger.IsDefaulter = driverrec.Status == MasterStatus.BlackListed;
                                    driverrec.fk_Ledger.ObjectState = ObjectState.Modified;
                                    driverrec.ObjectState = ObjectState.Modified;
                                }
                                break;
                        }
                        driverrepo.AddOrUpdate(driverrec);
                    }
                    break;

                case "LedgerRole":
                    var ledgerRole = entry.Entity as LedgerRole;
                    if (ledgerRole != null)
                    {
                        var clsRepo = this.Set<ObjectClassMap>();
                        if (ledgerRole.ObjectState == ObjectState.Added)
                        {
                            #region Add Default Class

                            var account = (ledgerRole.fk_Ledger == null || ledgerRole.fk_Ledger == default(Ledger))
                                ? this.Set<Ledger>()
                                    .Where(x => x.Id == ledgerRole.LedgerId)
                                    .Select(x => new { x.AccountRoleId, x.GroupId })
                                    .FirstOrDefault()
                                : new { ledgerRole.fk_Ledger.AccountRoleId, ledgerRole.fk_Ledger.GroupId };
                            if (account == null)
                                throw new BusinessException(ErrorCode.GLB106, "Core: Invalid Role Mapping");

                            var ctgQuery = this.Set<ObjectCategory>().Where(x =>
                               x.RoleId == ledgerRole.RoleId && (new long[] { 1145, 1146 }).Contains(x.RoleTypeId) && x.Objects.All(y => y.ObjectId != ledgerRole.LedgerId));
                            if (ctgQuery.Any())
                            {
                                var query = ctgQuery.SelectMany(x => x.ObjectClasses)
                                    .Where(x => x.ClassName == "All").Select(x => new
                                    {
                                        ClassId = x.Id,
                                        CategoryId = x.CategoryId,
                                        x.ClassName
                                    });
                                if (query.Any())
                                {
                                    var cls = query.ToList();
                                    var list = cls.Select(x => new ObjectClassMap
                                    {
                                        Id = 0,
                                        ObjectState = ObjectState.Added,
                                        ObjectId = ledgerRole.LedgerId,
                                        ClassId = x.ClassId,
                                        CategoryId = x.CategoryId
                                    }).ToList();
                                    clsRepo.AddRange(list);//Insert for new group
                                }
                            }

                            //var cls = this.Set<ObjectClass>()
                            //    .Where(
                            //        x =>
                            //            x.ClassName == "All" &&
                            //            (x.RoleId == account.GroupId || x.RoleId == account.AccountRoleId) &&
                            //            (new long[] { 1145, 1146 }).Contains(x.Category.RoleTypeId)&&!x.ObjectMappings.Any(z=>z.ObjectId==ledgerRole.LedgerId)).Select(x => new
                            //            {
                            //                ClassId = x.Id,
                            //                x.CategoryId
                            //            }).ToList();
                            //if (cls.Any())
                            //{
                            //    var list = cls.Select(x => new ObjectClassMap
                            //    {
                            //        Id = 0,
                            //        ObjectState = ObjectState.Added,
                            //        ObjectId = ledgerRole.LedgerId,
                            //        ClassId = x.ClassId,
                            //        CategoryId = x.CategoryId
                            //    }).ToList();
                            //    clsRepo.AddRange(list);
                            //}

                            #endregion Add Default Class
                        }

                        if (ledgerRole.ObjectState == ObjectState.Deleted)
                        {
                            this.Database.ExecuteSqlCommand(
                                $"DELETE FROM [dbo].[tObjectClassMap] WHERE CategoryId in(SELECT C.Id FROM[dbo].[mObjectCategory] C WHERE C.RoleTypeId= 1145 AND C.RoleId= {ledgerRole.RoleId}) AND ObjectId = {ledgerRole.LedgerId}");
                        }
                    }
                    break;

                case "DueTransactionLog":
                    var dueTransactionLog = entry.Entity as DueTransactionLog;
                    try
                    {
                        if (dueTransactionLog != null)
                        {
                            switch (dueTransactionLog.ObjectState)
                            {
                                case ObjectState.Added:
                                    var previousDue =
                                        this.Set<DueTransactionLog>()
                                            .OrderByDescending(x => x.PaidDate)
                                            .ThenByDescending(x => x.Id)
                                            .Include(x => x.fk_NextLog)
                                            .FirstOrDefault(
                                                x =>
                                                    x.DueTypeId == dueTransactionLog.DueTypeId && x.NextLogId == null &&
                                                    x.VehicleId == dueTransactionLog.VehicleId &&
                                                    x.StartDate <= dueTransactionLog.StartDate);

                                    if (previousDue != null)
                                    {
                                        var pnextdue = previousDue.fk_NextLog;
                                        if (pnextdue != null)
                                        {
                                            pnextdue.PreviousLogId = dueTransactionLog.Id;
                                            pnextdue.fk_PreviousLog = dueTransactionLog;
                                            dueTransactionLog.fk_NextLog = pnextdue;
                                            dueTransactionLog.NextLogId = pnextdue.Id;
                                            pnextdue.ObjectState = ObjectState.Modified;
                                        }
                                        dueTransactionLog.PreviousLogId = previousDue.Id;
                                        dueTransactionLog.fk_PreviousLog = previousDue;
                                        previousDue.NextLogId = dueTransactionLog.Id;
                                        previousDue.fk_NextLog = dueTransactionLog;
                                        previousDue.ObjectState = ObjectState.Modified;
                                    }
                                    break;

                                case ObjectState.Modified:
                                    try
                                    {
                                        var originalValue = entry.OriginalValues["StartDate"];
                                        if (originalValue != null && !originalValue.Equals(dueTransactionLog.StartDate))
                                        {
                                            throw new BusinessException(ErrorCode.GLB106,
                                                $"Core: Start Date Change for {dueTransactionLog.RefNo1} is not allowed.\r\n Help: Delete {dueTransactionLog.RefNo1} Record of from Transaction and Add it again i.e. it would be considered as new Entry.");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //Ignore
                                    }
                                    break;

                                case ObjectState.Deleted:
                                    DueTransactionLog d11 = dueTransactionLog.fk_PreviousLog ??
                                                            this.Set<DueTransactionLog>()
                                                                .OrderByDescending(x => x.PaidDate)
                                                                .ThenByDescending(x => x.Id)
                                                                .FirstOrDefault(
                                                                    x => x.NextLogId == dueTransactionLog.Id);

                                    if (d11 != null)
                                    {
                                        var d13 = dueTransactionLog.fk_NextLog ??
                                                  this.Set<DueTransactionLog>()
                                                      .OrderBy(x => x.PaidDate)
                                                      .ThenBy(x => x.Id)
                                                      .FirstOrDefault(x => x.Id == dueTransactionLog.NextLogId);
                                        //if (dueTransactionLog.fk_NextLog == null)
                                        //{
                                        //    dueTransactionLog.fk_NextLog =
                                        //        this.Set<DueTransactionLog>().OrderBy(x => x.PaidDate).ThenBy(x => x.Id)
                                        //            .FirstOrDefault(x => x.Id == dueTransactionLog.NextLogId);
                                        //}
                                        this.Set<DueTransactionLog>().Attach(d11);
                                        d11.NextLogId = dueTransactionLog.NextLogId;
                                        d11.fk_NextLog = d13;
                                        d11.ObjectState = ObjectState.Modified;
                                        if (d13 != null)
                                        {
                                            this.Set<DueTransactionLog>().Attach(d13);
                                            d13.ObjectState = ObjectState.Modified;
                                            d13.fk_PreviousLog = d11;
                                            d13.PreviousLogId = d11.Id;
                                        }

                                        dueTransactionLog.PreviousLogId = null;
                                        dueTransactionLog.fk_PreviousLog = null;
                                        dueTransactionLog.fk_NextLog = null;
                                        dueTransactionLog.NextLogId = null;
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetBaseException().Message.Equals("Sequence contains more than one element"))
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Integrity Check Raised an Error.");
                        }

                        throw;
                    }

                    break;

                case "TyreCheck":
                    var tyreCheck = entry.Entity as TyreCheck;

                    try
                    {
                        if (tyreCheck != null)
                        {
                            switch (tyreCheck.ObjectState)
                            {
                                case ObjectState.Added:
                                    var t1 =
                                        this.Set<TyreCheck>()
                                            .OrderByDescending(x => x.CheckDate)
                                            .ThenByDescending(x => x.Id)
                                            .Include(x => x.fk_NextLog)
                                            .FirstOrDefault(
                                                x =>
                                                    x.TyreId == tyreCheck.TyreId &&
                                                    x.CheckDate <= tyreCheck.CheckDate); //Previous Check

                                    if (t1 != null)
                                    {
                                        var t3 = t1.fk_NextLog; //Next Check
                                        if (t3 != null)
                                        {
                                            t3.PreviousLogId = tyreCheck.Id;
                                            t3.fk_PreviousLog = tyreCheck;
                                            tyreCheck.fk_NextLog = t3;
                                            tyreCheck.NextLogId = t3.Id;
                                            t3.ObjectState = ObjectState.Modified;
                                        }
                                        tyreCheck.PreviousLogId = t1.Id;
                                        tyreCheck.fk_PreviousLog = t1;
                                        t1.NextLogId = tyreCheck.Id;
                                        t1.fk_NextLog = tyreCheck;
                                        t1.ObjectState = ObjectState.Modified;
                                    }
                                    break;

                                case ObjectState.Modified:
                                    try
                                    {
                                        var originalValue = entry.OriginalValues["CheckDate"];
                                        if (originalValue != null && !originalValue.Equals(tyreCheck.CheckDate))
                                        {
                                            throw new BusinessException(ErrorCode.GLB106,
                                                $"Core: Check Date Change for Tyre Check is not allowed.\r\n Help: Delete Record of from Transaction and Add it again i.e. it would be considered as new Entry.");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //Ignore
                                    }
                                    break;

                                case ObjectState.Deleted:

                                    TyreCheck t11 = tyreCheck.fk_PreviousLog ??
                                                    this.Set<TyreCheck>()
                                                        .OrderByDescending(x => x.CheckDate)
                                                        .ThenByDescending(x => x.Id)
                                                        .FirstOrDefault(
                                                            x => x.NextLogId == tyreCheck.Id);

                                    if (t11 != null)
                                    {
                                        var t13 = tyreCheck.fk_NextLog ??
                                                  this.Set<TyreCheck>().OrderBy(x => x.CheckDate).ThenBy(x => x.Id)
                                                      .FirstOrDefault(x => x.Id == tyreCheck.NextLogId);
                                        this.Set<TyreCheck>().Attach(t11);
                                        t11.NextLogId = tyreCheck.NextLogId;
                                        t11.fk_NextLog = t13 ?? null;
                                        t11.ObjectState = ObjectState.Modified;
                                        if (t13 != null)
                                        {
                                            t13.ObjectState = ObjectState.Modified;
                                            t13.fk_PreviousLog = t11;
                                            t13.PreviousLogId = t11.Id;
                                            this.Set<TyreCheck>().Attach(t13);
                                        }

                                        tyreCheck.PreviousLogId = null;
                                        tyreCheck.fk_PreviousLog = null;
                                        tyreCheck.fk_NextLog = null;
                                        tyreCheck.NextLogId = null;
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetBaseException().Message.Equals("Sequence contains more than one element"))
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Integrity Check Raised an Error.");
                        }

                        throw;
                    }
                    break;
                
                case "CnChallan":
                    var chcn = entry.Entity as CnChallan;
                    if (chcn == null) break;
                    try
                    {
                        //TODO:Add Logic to Door to Door Delivery Mode[ConstantId 1472] where stock is not maintained.
                        new CnChallanCoreLogic().Bind(this).Execute(entry);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    break;

                case "CNMaster":
                    var cn = entry.Entity as CNMaster;

                    cn.ConstCurTypeId = Helper.ConstCurTypeId;

                    if (cn.CurRate == 0 && cn.CurTypeId != cn.ConstCurTypeId)
                    {
                        var cc = this.Set<CurrencyConversion>().Where(x => x.CurDate <= cn.CNDate && x.IsActive && x.CurTypeId == cn.CurTypeId).OrderByDescending(x => x.CurDate).FirstOrDefault();
                        if (cc != null)
                        {
                            cn.CurRate = cc.CurRate;
                        }
                    }

                    if (cn.CurTypeId == cn.ConstCurTypeId)
                    {
                        cn.CurRate = 1;
                    }

                    if (cn?.ObjectState == ObjectState.Modified)
                    {
                        decimal.TryParse(entry.OriginalValues["CNSubTotalII"]?.ToString(), out var previousFreight);
                        if (this.Set<CNBillLog>().Any(x => x.CNId == cn.Id && x.fk_Bill.fk_BillNature.CNBillTypeId == 1363) && cn.CNSubTotalII != previousFreight)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Once CN has been Billed Freight cannot be changed");
                        }
                        var oldtriplogid = (long?)entry.OriginalValues["TripLogId"];
                        if (oldtriplogid != cn.TripLogId)
                        {
                            if (oldtriplogid.GetValueOrDefault() > 0)
                            {
                                var chrepo = this.Set<CnChallan>();

                                if (chrepo.Any(x => x.TriplogId == oldtriplogid && x.CNId == cn.Id))
                                {
                                    var challancn = chrepo.Include(x => x.fk_Triplog)
                                        .Include(x => x.fk_Challan.CNChallans)
                                        .FirstOrDefault(
                                            x =>
                                                x.TriplogId == oldtriplogid &&
                                                x.CNId == cn.Id);
                                    challancn.ObjectState = ObjectState.Deleted;
                                    if (challancn.fk_Challan?.CNChallans != null && challancn.fk_Challan.CNChallans.Count == 1)
                                    {
                                        challancn.fk_Challan.ObjectState = ObjectState.Deleted;
                                        this.Set<ChallanMaster>().Remove(challancn.fk_Challan);
                                    }
                                    if (challancn.fk_Triplog != null)
                                    {
                                        challancn.fk_Triplog.LoadingQty = challancn.fk_Triplog.LoadingQty - challancn.Qty;
                                        if (challancn.fk_Triplog.LoadingQty < 0)
                                            challancn.fk_Triplog.LoadingQty = 0;
                                        challancn.fk_Triplog.LoadedWeight = challancn.fk_Triplog.LoadedWeight -
                                                                            challancn.Weight;
                                        if (challancn.fk_Triplog.LoadedWeight < 0)
                                            challancn.fk_Triplog.LoadedWeight = 0;
                                        challancn.fk_Triplog.ObjectState = ObjectState.Modified;
                                        this.Set<VehicleMovementLog>().AddOrUpdate(challancn.fk_Triplog);
                                    }
                                    if (challancn.fk_Challan != null &&
                                        challancn.fk_Challan.ObjectState != ObjectState.Deleted)
                                    {
                                        challancn.fk_Challan.Quantity = challancn.fk_Challan.Quantity - challancn.Qty;
                                        if (challancn.fk_Challan.Quantity < 0)
                                            challancn.fk_Challan.Quantity = 0;
                                        challancn.fk_Challan.Weight = challancn.fk_Challan.Weight - challancn.Weight;
                                        if (challancn.fk_Challan.Weight < 0)
                                            challancn.fk_Challan.Weight = 0;
                                        challancn.fk_Challan.ObjectState = ObjectState.Modified;
                                        this.Set<ChallanMaster>().AddOrUpdate(challancn.fk_Challan);
                                    }
                                    this.Set<CnChallan>().Remove(challancn);
                                    this.Database.ExecuteSqlCommand(
                                        $"EXEC Proc_DeleteOutStockLog @ChallanCnId={challancn.Id}");
                                }
                            }
                            try
                            {
                                this.ChangeTracker.DetectChanges();
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    break;

                case "VehicleCardMapping":
                    new CardCoreLogic().Bind(this).Execute(entry);
                    break;

                case "VehicleTrailorMapping":
                    new TrailorMappingCoreLogic().Bind(this).Execute(entry);
                    break;

                case "SalesLog":
                    new SalesLogCoreLogic().Bind(this).Execute(entry);
                    break;

                case "CNBillLog":
                    new CNBillLogCoreLogic().Bind(this).Execute(entry);
                    break;

                case "CNBill":                    
                    new CNBillCoreLogic().Bind(this).Execute(entry);
                    break;

                case "CNBillPaymentLog":
                    new CNBillPaymentLogCoreLogic().Bind(this).Execute(entry);
                    break;

                case "TyreMaster":
                    new TyreMasterCoreLogic().Bind(this).Execute(entry);
                    break;

                case "BatteryMaster":
                    new BatteryMasterCoreLogic().Bind(this).Execute(entry);
                    break;

                case "CNStockLog":
                    new CNStockLogCoreLogic().Bind(this).Execute(entry);
                    break;
            }
            var entity = entry.Entity as AuditableEntity;
            if (entity != null)
            {
                try
                {
                    var voucherid = entry.Entity.GetPropertyValue<long>("VoucherId");
                    //var voucherid = entry.Property("VoucherId")?.CurrentValue as long?;
                    if (voucherid > 0 && !ChangeTracker.Entries<Voucher>().Any(x => x.Entity.Id == voucherid))
                    {
                        var vrepo = Set<Voucher>();
                        if (vrepo.Any(x => x.Id == voucherid))
                        {
                            if (entity.ObjectState == ObjectState.Deleted)
                            {
                                var voucher = vrepo.Find(voucherid);
                                var dbvoucherentry = this.Entry(voucher);
                                CheckVoucher(dbvoucherentry,true);
                            }
                            //if (entity.ObjectState == ObjectState.Modified|| entity.ObjectState == ObjectState.Deleted)
                            //{
                            //    var voucher = vrepo.Find(voucherid);                                
                            //    voucher.ObjectState = entity.ObjectState;
                            //    var dbvoucherentry = this.Entry(voucher);
                            //    dbvoucherentry.State = entry.State;
                            //    CheckVoucher(dbvoucherentry);
                            //}
                        }
                    }
                }
                catch (BusinessException)
                {
                    throw;
                }
                catch
                {
                    //Ignore
                }
            }

            #region Stationary Archive Logics

            try
            {
                var pageid = entity?.PageId;
                if (pageid > 0 && entity.ObjectState != ObjectState.Unchanged)
                {
                    if (_configLog2Archive == null)
                    {
                        _configLog2Archive =
                                            new MapperConfiguration(cfg => cfg.CreateMap<StationeryBookLog, StationeryBookLogArchive>())
                                                .CreateMapper();
                        _configArchive2Log =
                            new MapperConfiguration(cfg => cfg.CreateMap<StationeryBookLogArchive, StationeryBookLog>())
                                .CreateMapper();
                    }

                    var bookRepo = this.Set<StationeryBookLog>();
                    var archRepo = this.Set<StationeryBookLogArchive>();
                    switch (entity.ObjectState)
                    {
                        case ObjectState.Added:
                            var page1 = bookRepo.Find(pageid);
                            
                            if (page1 == null)
                            {
                                var page2 = archRepo.Where(x => x.Id == pageid).Select(k =>  new { k.BookId }).FirstOrDefault();
                                if (page2 != null)
                                {
                                    /*Sliently increase one page*/
                                    page1 = bookRepo.Where(x => x.BookId == page2.BookId).OrderBy(f => f.PageNo).FirstOrDefault();
                                }
                            }

                            if (page1 == null)
                            {
                                throw new BusinessException(ErrorCode.GLB106,
                                    $"Core1:Stationary Book Page has been consumed by someone else.\r\n Please use another one. PageId:{pageid}");
                            }
                            if (page1 != null &&
                                (page1.NatureId.GetValueOrDefault() == 1233 || page1.NatureId.GetValueOrDefault() == 0))/*Book*/
                            //Process only if book was not of Auto and Manual Book Type
                            {
                                var archive1 = _configLog2Archive.Map<StationeryBookLogArchive>(page1);
                                var book = this.Set<StationeryBook>().FirstOrDefault(x => x.Id == page1.BookId);
                                book.IsLocked = true;
                                book.IsUsed = true;
                                book.ObjectState = ObjectState.Modified;
                                this.Set<StationeryBook>().AddOrUpdate(book);
                                archive1.ObjectState = ObjectState.Added;
                                page1.ObjectState = ObjectState.Deleted;
                                bookRepo.Remove(page1);
                                archRepo.Add(archive1);
                            }
                            else if (page1.NatureId == 1232)/*Auto*/
                            {
                                try
                                {
                                    var book = this.Set<StationeryBook>().FirstOrDefault(x => x.Id == page1.BookId);
                                    //var newbooklog = page1.Clone();
                                    //newbooklog.Id = 0;
                                    //newbooklog.ObjectState = ObjectState.Added;
                                    book.PreviousUsedPage = page1.PageNo;
                                    book.ObjectState = ObjectState.Modified;
                                    //newbooklog.fk_StationeryBook = book;
                                    this.Set<StationeryBook>().AddOrUpdate(book);
                                    book.IsLocked = true;
                                    book.IsUsed = true;
                                    var oldpagenumber =string.IsNullOrWhiteSpace(book.Prefix)? book.PreviousUsedPage: book.PreviousUsedPage.Replace(book.Prefix, "");
                                    if (!string.IsNullOrWhiteSpace(oldpagenumber))
                                    {
                                        long oldautonumber = 0;
                                        long.TryParse(oldpagenumber, out oldautonumber);
                                        //newbooklog.PageNo = $"{(string.IsNullOrWhiteSpace(book.Prefix) ? "" : book.Prefix)}{(oldautonumber + 1).ToString().PadLeft(book.NoOfDigits, '0')}";
                                        page1.PageNo = $"{(string.IsNullOrWhiteSpace(book.Prefix) ? "" : book.Prefix)}{(oldautonumber + 1).ToString().PadLeft(book.NoOfDigits, '0')}";
                                        //bookRepo.Add(newbooklog);
                                    }
                                    //bookRepo.Remove(page1);
                                    page1.ObjectState = ObjectState.Modified;
                                    bookRepo.AddOrUpdate(page1);
                                }
                                catch (Exception ex)
                                {
                                    throw new BusinessException(ErrorCode.GLB106, $"Core3:Stationary Auto Page id {pageid} has been consumed by someone else.\r\n Please use another one.\r\nInternal Error:{ex.GetBaseException().Message}");
                                }
                            }
                            else if (page1.NatureId == 1232)/*Serial*/
                            {
                                try
                                {
                                    var book = this.Set<StationeryBook>().FirstOrDefault(x => x.Id == page1.BookId);
                                    book.PreviousUsedPage = page1.PageNo;
                                    var newbooklog = page1.Clone();
                                    var archive1 = _configLog2Archive.Map<StationeryBookLogArchive>(page1);
                                    archive1.ObjectState = ObjectState.Added;
                                    page1.ObjectState = ObjectState.Deleted;
                                    archRepo.Add(archive1);
                                    bookRepo.Remove(page1);

                                    newbooklog.Id = 0;
                                    newbooklog.ObjectState = ObjectState.Added;

                                    book.ObjectState = ObjectState.Modified;
                                    newbooklog.fk_StationeryBook = book;
                                    this.Set<StationeryBook>().AddOrUpdate(book);
                                    book.IsLocked = true;
                                    book.IsUsed = true;
                                    //var oldpagenumber = book.PreviousUsedPage.Replace(book.Prefix, "");
                                    var oldpagenumber = string.IsNullOrWhiteSpace(book.Prefix) ? book.PreviousUsedPage : book.PreviousUsedPage.Replace(book.Prefix, "");
                                    if (!string.IsNullOrWhiteSpace(oldpagenumber))
                                    {
                                        long oldautonumber = 0;
                                        long.TryParse(oldpagenumber, out oldautonumber);
                                        newbooklog.PageNo = $"{(string.IsNullOrWhiteSpace(book.Prefix) ? "" : book.Prefix)}{(oldautonumber + 1).ToString().PadLeft(book.NoOfDigits, '0')}";
                                        bookRepo.Add(newbooklog);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new BusinessException(ErrorCode.GLB106, $"Core4:Stationary Serial Page Id {pageid} has been consumed by someone else.\r\n Please use another one.\r\nInternal Error:{ex.GetBaseException().Message}");
                                }
                            }
                            break;

                        case ObjectState.Modified:
                            long? actualPageId = 0;
                            try
                            {
                                actualPageId = (long?)entry.Property("PageId").OriginalValue;
                            }
                            catch (Exception)
                            {
                                actualPageId = (long?)entry.GetDatabaseValues()?["PageId"];
                            }

                            if (!actualPageId.GetValueOrDefault().Equals(pageid))
                            {
                                var oldarch = archRepo.Find(actualPageId);
                                if (oldarch != null && (oldarch.NatureId.GetValueOrDefault() == 1233 || oldarch.NatureId.GetValueOrDefault() == 0)) //Process only if book was not of Auto and Manual Book Type
                                {
                                    #region Archive to Log

                                    if (oldarch != null)
                                    {
                                        var oldpage = _configArchive2Log.Map<StationeryBookLog>(oldarch);
                                        oldpage.ObjectState = ObjectState.Added;
                                        oldpage.Id = 0;
                                        oldarch.ObjectState = ObjectState.Deleted;
                                        archRepo.Remove(oldarch);
                                        bookRepo.Add(oldpage);
                                    }

                                    #endregion Archive to Log

                                    #region Log and Archive

                                    var newpage = bookRepo.Find(pageid);
                                    if (newpage == null)
                                        throw new BusinessException(ErrorCode.GLB106,
                                            $"Core5:Stationary Page Id {pageid} has been consumed by someone else.\r\n Please use another one.");
                                    var newarchive = _configLog2Archive.Map<StationeryBookLogArchive>(newpage);
                                    newarchive.ObjectState = ObjectState.Added;
                                    newpage.ObjectState = ObjectState.Deleted;
                                    bookRepo.Remove(newpage);
                                    archRepo.Add(newarchive);

                                    #endregion Log and Archive
                                }
                            }
                            break;

                        case ObjectState.Deleted:
                            var archive = archRepo.FirstOrDefault(x => x.Id == pageid);
                            if (archive != null && (archive.NatureId.GetValueOrDefault() == 1233 || archive.NatureId.GetValueOrDefault() == 0)) //Process only if book was not of Auto and Manual Book Type
                            {
                                if (archive == null)
                                {
                                    break;
                                }
                                var page = _configArchive2Log.Map<StationeryBookLog>(archive);
                                page.ObjectState = ObjectState.Added;
                                archive.ObjectState = ObjectState.Deleted;
                                archRepo.Remove(archive);
                                page.Id = 0;
                                bookRepo.Add(page);
                            }
                            break;
                    }
                }
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            #endregion Stationary Archive Logics
        }
        private void PostDataCommit()
        {
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
                switch (dbEntityEntry.Entity.GetType().Name)
                {
                    case "Voucher":
                        new VoucherCoreLogic().Bind(this).Execute(dbEntityEntry);
                        break;
                }
            }
        }
        private void SyncObjectsStatePreCommit()
        {
            //Check if All the ledgers are mapped to AccountGroup
            //if (Helper.GetFinanceStatus() != FinanceStatus.NA)
            //{
            var allVdIds =
                ChangeTracker.Entries<VoucherDetail>().Select(x => x.Entity.AccountId).Distinct().ToList();
            if (allVdIds.Any())
            {
                var invalidAccounts =
                    this.Set<Ledger>()
                        .Where(x => allVdIds.Contains(x.Id) && x.GroupId == null)
                        .Select(x => x.AccountName)
                        .ToList().JoinStrings(",");
                if (!string.IsNullOrWhiteSpace(invalidAccounts))
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"{invalidAccounts} Ledgers are not mapped to Account Group.");
                }
            }

            //}
            //var deleted =
            //    ChangeTracker.Entries()
            //        .Where(x => ((TrackoApi.Models.Base.Entity)x.Entity).ObjectState == ObjectState.Deleted).ToList();
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
                //var entity = dbEntityEntry.Entity as TrackoApi.Models.Base.Entity;
                //if (entity != null)
                //{
                //    Debug.Assert(entity.Id>0&&(dbEntityEntry.State==EntityState.Modified|| dbEntityEntry.State == EntityState.Modified));
                //}
                //if (!(dbEntityEntry.Entity is TrackoApi.Models.Base.Entity))
                //{
                //    PreCoreLogic(dbEntityEntry);
                //    continue;
                //}

                //if (dbEntityEntry != null && dbEntityEntry.Property("ConstCurTypeId")!=null) {
                //    dbEntityEntry.Property("ConstCurTypeId").CurrentValue = Helper.ConstCurTypeId;
                //}
                PreCoreLogic(dbEntityEntry);
            }
            ChangeTracker.DetectChanges();
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
                try
                {
                    //dbEntityEntry.State = StateHelper.ConvertState(((IEntity)dbEntityEntry.Entity).ObjectState);
                    if (!(dbEntityEntry.Entity is IEntity entity)) continue;
                    if (dbEntityEntry.State == EntityState.Unchanged && entity.ObjectState == ObjectState.Unchanged) continue;
                    var state = StateHelper.ConvertState(entity.ObjectState);
                    if (dbEntityEntry.State != state) this.Entry(dbEntityEntry.Entity).State = state;
                    switch (dbEntityEntry.State)
                    {
                        case EntityState.Added:

                            try
                            {
                                if (entity.GetType().Name != "StationeryBookLogArchive" && entity.Id > 0 /*&& dbEntityEntry.Property("Id").OriginalValue != null*/)
                                {
                                    if (dbEntityEntry.GetDatabaseValues() != null)
                                    {
                                        dbEntityEntry.State = EntityState.Modified;
                                    }
                                    //ExceptionlessClient.Default.CreateEvent().AddObject(dbEntityEntry).SetMessage("Added").Submit();
                                }
                            }
                            catch (Exception ex)
                            {
                            }

                            break;

                        case EntityState.Deleted:
                            if (entity.Id == 0)
                            {
                                dbEntityEntry.State = EntityState.Unchanged;
                                //ExceptionlessClient.Default.CreateEvent().AddObject(dbEntityEntry).SetMessage("Deleted").Submit();
                            }
                            break;

                        case EntityState.Modified:
                            if (entity.Id == 0)
                            {
                                dbEntityEntry.State = EntityState.Added;
                                // ExceptionlessClient.Default.CreateEvent().AddObject(dbEntityEntry).SetMessage("Modified").Submit();
                            }

                            //if (entity.ModifiedProperties.Any())
                            //{
                            //}
                            break;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                if (RulesAreOn == null)
                {
                    RulesAreOn = GetApiConfig<int>("RunAPIRules") == 1;
                }
                if (RulesAreOn.GetValueOrDefault(false))
                {
                    switch (dbEntityEntry.State)
                    {
                        case EntityState.Added:
                        case EntityState.Deleted:
                        case EntityState.Modified:
                            var typeed = dbEntityEntry.Entity.GetType();
                            var rulekey = $"{typeed.Name}_ValidateCore";
                            try
                            {
                                var rules = this.Set<Rule>().Where(x => x.IsActive && x.RuleKey == rulekey)
                                    .FromCache(CachePolicy.WithSlidingExpiration(TimeSpan.FromHours(3)))
                                    ?.ToList();
                                if (rules != null)
                                {
                                    if (rules.Any())
                                    {

                                        var entitytype = dbEntityEntry.Entity as Entity;
                                        //MethodInfo castMethod = this.GetType().GetMethod("Cast").MakeGenericMethod(typeed);
                                        //var entitytype = castMethod.Invoke(null, new object[] { dbEntityEntry.Entity });
                                        var logics2Apply = rules.Where(x =>
                                            x.RuleNature == RuleNature.Assignment &&
                                            (string.IsNullOrWhiteSpace(x.ValidationDefination) ||
                                             dbEntityEntry.VaidateDbEntry(entitytype, this, x.ValidationDefination,x.Id,this.DbName)));

                                        foreach (var rule in logics2Apply)
                                        {
                                            //entitytype.ApplyRule(this, rule.AssignmentDefination, rule.Id,this.Database.Connection.Database);
                                            dbEntityEntry.ApplyDbRule(entitytype, this, rule.AssignmentDefination, rule.Id, this.DbName);
                                        }
                                        var failedvalidations = rules.Where(x =>
                                            !string.IsNullOrWhiteSpace(x.ValidationDefination) &&
                                            x.RuleNature == RuleNature.Validation &&
                                            !dbEntityEntry.VaidateDbEntry(entitytype, this, x.ValidationDefination, x.Id, this.DbName));
                                        var errormessage = string.Empty;
                                        int errornum = 0;
                                        foreach (var failedvalidation in failedvalidations)
                                        {
                                            errormessage +=
                                                $"\n{(++errornum).ToString()}) " + failedvalidation.FailedMessage;
                                        }

                                        if (!string.IsNullOrEmpty(errormessage))
                                        {
                                            throw new BusinessException(ErrorCode.GLB106, errormessage);
                                        }
                                    }
                                }
                            }
                            catch (BusinessException)
                            {
                                throw;
                            }
                            catch (Exception)
                            {
                                //Ignore
                            }

                            break;
                    }
                }
            }

            //foreach (var entry in ChangeTracker.Entries().Where(x => (x.Entity is TrackoApi.Models.Base.Entity || x.Entity is IAuditableEntity) && ((TrackoApi.Models.Base.Entity)x.Entity).ObjectState != ObjectState.Unchanged))
            //{
            //    try
            //    {
            //        entry.State = StateHelper.ConvertState(((IEntity) entry.Entity).ObjectState);
            //    }
            //    catch (Exception)
            //    {
            //        // ignored
            //    }
            //}
            UpdateSessionInfo();
        }
        public T Cast<T>(object o)
        {
            return (T)o;
        }
        private ObjectQuery<TEntity> ToObjectQuery<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            var objectQuery = query as ObjectQuery<TEntity>;
            if (objectQuery != null)
            {
                return objectQuery;
            }
            var dbQuery = query as DbQuery<TEntity>;
            if (dbQuery != null)
            {
                //access internal property InternalQuery
                var internalQuery =
                    dbQuery.GetType()
                        .GetProperty("InternalQuery", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(dbQuery);
                if (internalQuery != null)
                {
                    //access internal property ObjectQuery
                    objectQuery =
                        internalQuery.GetType().GetProperty("ObjectQuery").GetValue(internalQuery) as
                            ObjectQuery<TEntity>;
                    return objectQuery;
                }
            }
            throw new ArgumentException("IQuerable must be an ObjectQuery or result of a DbQuery");
        }

        private void UpdateSessionInfo()
        {
            //if (SessionId <0) return;
            //if (HttpContext.Current.User == null) return;
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
            //var ctx = (ClaimsPrincipal)HttpContext.Current.User;
            //var sessionIdObj = ctx.Claims.FirstOrDefault(x => x.Type == "SessionId");
            //if (sessionIdObj == null) return;
            foreach (var entry in modifiedEntries)
            {
                if (entry.Entity is IAuditableEntity entity)
                {
                    //var sessionId = long.Parse(sessionIdObj.Value);
                    var now = DateTime.Now;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedSessionId = SessionId;
                        entity.SecuredByTenantId = TenantId;
                        entity.CreatedDOE = now;
                    }
                    else
                    {
                        Entry(entity).Property(x => x.CreatedSessionId).IsModified = false;
                        Entry(entity).Property(x => x.CreatedDOE).IsModified = false;
                        entity.ModifiedDOE = now;
                        entity.ModifiedSessionId = SessionId;
                        if (string.IsNullOrWhiteSpace(entity.SecuredByTenantId))
                        {
                            entity.SecuredByTenantId = TenantId;
                        }
                    }
                    if (default(DateTime) == entity.CreatedDOE)
                    {
                        entity.CreatedDOE = DateTime.Now;
                    }
                }
                else
                {
                    if (!(entry.Entity is IAuditableInfraEntity infraEntity)) continue;
                    //var sessionId = long.Parse(sessionIdObj.Value);
                    var now = DateTime.Now;

                    if (entry.State == EntityState.Added)
                    {
                        infraEntity.CreatedSessionId = SessionId;
                        infraEntity.CreatedDOE = now;
                        infraEntity.SecuredByTenantId = TenantId;
                    }
                    else
                    {
                        Entry(infraEntity).Property(x => x.CreatedSessionId).IsModified = false;
                        Entry(infraEntity).Property(x => x.CreatedDOE).IsModified = false;
                        infraEntity.ModifiedDOE = now;
                        infraEntity.ModifiedSessionId = SessionId;
                    }
                    if (default(DateTime) == infraEntity.CreatedDOE)
                    {
                        infraEntity.CreatedDOE = DateTime.Now;
                    }
                    if (default(DateTime) == infraEntity.ModifiedDOE)
                    {
                        infraEntity.ModifiedDOE = null;
                    }
                }
            }
        }
        #region Core Entities

        /// <summary>
        /// Gets or sets the API access controls.
        /// </summary>
        /// <value>The API access controls.</value>
        public DbSet<ApiRolePermission> ApiAccessControls { get; set; }

        /// <summary>
        /// Gets or sets the API modules.
        /// </summary>
        /// <value>The API modules.</value>
        public DbSet<ApiViewModule> ApiModules { get; set; }

        /// <summary>
        /// Gets or sets the API sessions.
        /// </summary>
        /// <value>The API sessions.</value>
        public DbSet<ApiSession> ApiSessions { get; set; }

        /// <summary>
        /// Gets or sets the API views.
        /// </summary>
        /// <value>The API views.</value>
        public DbSet<ApiView> ApiViews { get; set; }

        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>The clients.</value>
        public DbSet<ApiAppClient> Clients { get; set; }

        /// <summary>
        /// Gets or sets the record access logs.
        /// </summary>
        /// <value>The record access logs.</value>
        public DbSet<ApiRecordAccessLog> RecordAccessLogs { get; set; }

        /// <summary>
        /// Gets or sets the refresh tokens.
        /// </summary>
        /// <value>The refresh tokens.</value>
        public DbSet<ApiRefreshToken> RefreshTokens { get; set; }
        /// <summary>
        /// Gets or sets the resource access logs.
        /// </summary>
        /// <value>The resource access logs.</value>
        public DbSet<ApiResourceAccessLog> ResourceAccessLogs { get; set; }
        
        //public new IDbSet<T> Set<T>() where T : class
        //{
        //    return base.Set<T>();
        //}

        #endregion Core Entities
    }
}