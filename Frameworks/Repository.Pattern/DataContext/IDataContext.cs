using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace Repository.Pattern.DataContext
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        Guid InstanceId { get; }
        bool Disposed { get;}
        void SyncObjectsStatePostCommit();
        /// <summary>
        /// gets and sets Applications that would Access Api
        /// </summary>
        DbSet<ApiAppClient> Clients { get; set; }
        /// <summary>
        /// Gets and Sets Refresh Token Pool
        /// </summary>
        DbSet<ApiRefreshToken> RefreshTokens { get; set; }
        DbSet<FaultVersionLog> FaultVersions { get; set; }
        /// <summary>
        /// Gets and Sets Permissions for Resources Provided By Api
        /// </summary>
        DbSet<ApiRolePermission> ApiAccessControls { get; set; }
        /// <summary>
        /// Gets and Sets Manual Api Session
        /// </summary>
        DbSet<ApiSession> ApiSessions { get; set; }
        /// <summary>
        /// Gets and Sets Recent Transaction Access Logs Per/User Per/Transaction
        /// </summary>
        DbSet<ApiRecordAccessLog> RecordAccessLogs { get; set; }
        /// <summary>
        /// Gets and Sets Api Users
        /// </summary>
        IDbSet<ApiUser> Users { get; set; }
        /// <summary>
        /// Gets and Sets Api Roles
        /// </summary>
        IDbSet<ApiRole> Roles { get; set; }
        /// <summary>
        /// Gets and Sets Resource Access Log for Witch Permission is Applied
        /// </summary>
        DbSet<ApiResourceAccessLog> ResourceAccessLogs { get; set; }
        //List<ApiRecordAccessLog> PendingAudits { get; set; }
            /// <summary>
        /// Gets and Sets View Master
        /// </summary>
        DbSet<ApiView> ApiViews { get; set; }
        /// <summary>
        /// Gets and Sets View Modules
        /// </summary>
        DbSet<ApiViewModule> ApiModules { get; set; }
        bool RequireUniqueEmail { get; set; }
        Database Database { get; }
        DbChangeTracker ChangeTracker { get; }
        DbContextConfiguration Configuration { get; }
        DbSet<ConversationGroup> ConversationGroups { get; set; }
        DbSet<UserConnection> UserConnections { get; set; }
        IEnumerable<DbEntityValidationResult> GetValidationErrors();
        DbEntityEntry Entry(object entity);
        DbSet<T> Set<T>() where T : class;
        DbSet<AccountParentChild> AccountGroupChildren { get; set; }
        DbSet<VDRBalance> VDRBalances { get; set; }
        int Delete<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;
        int Delete<TEntity>(IQueryable<TEntity> query) where TEntity : class;
        int Delete<TEntity>(ObjectQuery<TEntity> query) where TEntity : class;
        T GetApiConfig<T>(string key) where T : struct;
        T GetApiConfig<T>(string key, T defaultValue) where T : struct;
        string GetApiConfig(string key);
        T GetApiClientConfig<T>(string key) where T : struct;
        T GetApiClientConfig<T>(string key, T defaultValue) where T : struct;
        long GetDTSStatusIdByDateId(long dateId);
        Task<string> ValidateTLDateRangeOverlap(DateTime tripStartDate, DateTime? tripEndDate, long ownvehicleid = 0, long hirevehicleid = 0, long triplogId = 0, long triptype = 1158, long tripnature = 0);
        int ExecuteProcedure(string sql, params object[] parameters);
    }
}