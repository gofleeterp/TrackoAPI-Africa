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
using TrackoApi.Models.Global.DTS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IDTSStatusMappingService : IService<DTSStatusMapping>
    {
        
    }
    public class DTSStatusMappingService : Service<DTSStatusMapping>, IDTSStatusMappingService
    {
        private readonly IRepositoryAsync<DTSStatusMapping> _repository;
        public DTSStatusMappingService(IRepositoryAsync<DTSStatusMapping> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public override void Delete(DTSStatusMapping entity)
        {
           entity.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
