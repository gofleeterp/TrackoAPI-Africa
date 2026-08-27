using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core.Helpers;

namespace Repository.Pattern.Ef6.DbInterceptors
{
    internal class DiscriminatorColumnInterceptor : DbCommandInterceptor
    {
        private readonly string tenentId;
        public DiscriminatorColumnInterceptor()
        {
            this.tenentId = Helper.LoggedInTenantId;
        }
        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            command.CommandText = $"USE DiscriminatorDB {command.CommandText}";

            if (command.CommandText.Contains("WHERE"))
            {
                command.CommandText += $" AND Tenant = '{this.tenentId}'";
            }
            else
            {
                command.CommandText += $" WHERE Tenant = '{this.tenentId}'";
            }
            base.ReaderExecuting(command, interceptionContext);
        }
        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            command.CommandText = $"USE DiscriminatorDB {command.CommandText}";

            if (command.CommandText.Contains("WHERE"))
            {
                command.CommandText += $" AND Tenant = '{this.tenentId}'";
            }
            else
            {
                command.CommandText += $" WHERE Tenant = '{this.tenentId}'";
            }
            base.ScalarExecuting(command, interceptionContext); 
        }
        
    }
}
