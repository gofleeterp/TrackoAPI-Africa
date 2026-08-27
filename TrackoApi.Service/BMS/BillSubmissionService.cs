using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Extensions;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.DTS;
using TrackoAPI.Code.Logics.BMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IBillSubmissionService : IService<BillSubmission>
    {
        
    }
    public class BillSubmissionService : Service<BillSubmission>, IBillSubmissionService
    {
        private readonly IRepositoryAsync<BillSubmission> _repository;
        public BillSubmissionService(IRepositoryAsync<BillSubmission> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(BillSubmission entity)
        {
            if (_repository.GetConfigValue<int>("IsCNTrackEnabled") != 0)
            {
                var statusid = _repository.UOW.Context.GetDTSStatusIdByDateId(1580);
                if (statusid == 0) return;
                var nextstatusid = _repository.UOW.Context.GetDTSStatusIdByDateId(1567);
                
                var cnlogs = _repository.GetRepository<CNDTSStatusLog>().Queryable().Where(x=>x.StatusId==nextstatusid||x.StatusId==statusid);
                var query =
                    (from b in _repository.GetRepository<CNBill>()
                        .Queryable()
                        .Where(x => x.CoverNoteId == entity.Id)
                        .SelectMany(x => x.BillLogs, (p, c) => c)
                        .Select(x => x.CNId)
                    join cd in cnlogs on b equals cd.CNId
                    orderby cd.Id descending 
                    select cd.Id).ToList();
               // var cndts = cnlogs.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).Where(x => x.fk_CN.BillId == entity.Id && (x.StatusId == statusid || x.StatusId == nextstatusid)).ToList();
                foreach (var cn in query)
                {
                    try
                    {
                        _repository.ExecuteSql($"EXEC [dbo].[Proc_TRANS_1555_Delete]{cn}");
                    }
                    catch (SqlException e)
                    {
                        throw new BusinessException(e);
                    }
                    
                }
            }
            _repository.ExecuteSql($"UPDATE [dbo].[tCNBillMaster] SET CoverNoteId=NULL WHERE CoverNoteId={entity.Id}");
            base.Delete(entity);
        }
    }
}
