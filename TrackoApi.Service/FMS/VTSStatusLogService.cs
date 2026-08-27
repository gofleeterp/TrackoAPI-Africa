using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVTSStatusLogService : IService<VTSStatusLog>
    {
        
    }
    public class VTSStatusLogService : Service<VTSStatusLog>, IVTSStatusLogService
    {
        private readonly IRepositoryAsync<VTSStatusLog> _repository;
        public VTSStatusLogService(IRepositoryAsync<VTSStatusLog> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public override void Delete(VTSStatusLog entity)
        {
            entity.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
