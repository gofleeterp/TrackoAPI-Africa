using EntityFramework.Extensions;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;

namespace Tenant
{
    public class ConnectionHelper
    {
        /// <exception cref="BusinessException">Session Expired.</exception>
        public static string GetConnection(IGlobalStore globalStore)
        {
            try
            {
                if (Helper.HostedOnPremise)
                {
                    var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                    if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");

                    return connection;
                }

                var ctx = HttpContext.Current;
                var connectionString = ctx?.GetOwinContext()?.Get<string>("as:tenantConnection");
                if (!string.IsNullOrWhiteSpace(connectionString)) return connectionString;
                var cru = (ClaimsPrincipal)ctx?.User;
                var claim = cru?.Claims?.FirstOrDefault(x => x.Type == "TenantId");
                if (claim == null)
                {
                    var tenanId = ctx?.GetOwinContext()?.Get<string>("as:tenantid");
                    if (!string.IsNullOrWhiteSpace(tenanId))
                    {
                        connectionString= globalStore.GetOrAddConnectionString(tenanId, GetConnectionFromDatabase);
                    }
                }
                if (!string.IsNullOrWhiteSpace(claim?.Value))
                {
                    connectionString= globalStore.GetOrAddConnectionString(claim?.Value, GetConnectionFromDatabase);
                }
                else
                {
                    throw new BusinessException("No Connection String");
                }
                return connectionString;
            }
            catch (System.Exception ex)
            {
                throw new BusinessException(ErrorCode.GLB109, ex.GetBaseException().Message);
            }
        }

        public static string GetConnectionByTenentId(IGlobalStore globalStore, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || Helper.HostedOnPremise) return GetConnection(globalStore);
            return globalStore.GetOrAddConnectionString(tenantId, GetConnectionFromDatabase);
        }

        private static string GetConnectionFromDatabase(string tenantId)
        {

            if (!Helper.HostedOnPremise)
            {


                using (var tct = new TenantDbContext())
                {
                    var tenant = tct.Tenants.Where(x => x.Id == tenantId).Select(x => x.ConnectionString).FromCacheFirstOrDefault(tags: new List<string>() { "integration" });


                    return tenant;
                }
            }

            var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
            if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
            return connection;
        }
    }
}