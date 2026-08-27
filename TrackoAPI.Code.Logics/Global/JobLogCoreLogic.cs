using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Repository.Pattern.DataContext;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global.CronJobs;

namespace TrackoAPI.Code.Logics.Global
{
    public class JobLogCoreLogic : IBaseLogic
    {
        //protected static JobLogCoreLogic _Instance;
        //public static JobLogCoreLogic Instance => _Instance ?? (_Instance = new JobLogCoreLogic());

        protected IDataContextAsync _db;
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            return this;
        }


        public void Execute(DbEntityEntry entry)
        {
            Execute(entry, false);
            SaveAfterPostLogic = false;
        }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            if (!(entry.Entity is JobLog entity)) return;
            if (isPostLogicCall) PostLogic(entity);
        }

        private void PostLogic(JobLog entity)
        {
            //if (entity.Id > 0)
            //{
            //    var tenant = HttpContext.Current.GetOwinContext().Get<TenantDbContext>(typeof(TenantDbContext).Name);

            //    var jobtrack = new JobTrack//TODO:Model has been changed due to External Integration must reevoluate this code
            //    {
            //        JobLogId = string.IsNullOrWhiteSpace(entity.JobId) ? (entity.JobId = Guid.NewGuid().ToString("N")) : entity.JobId,
            //        TenantId = Helper.LoggedInTenantId
            //    };
            //    tenant.Jobs.Add(jobtrack);
            //    tenant.SaveChanges();
            //}
        }

        public bool SaveAfterPostLogic { get; set; }
    }
}
