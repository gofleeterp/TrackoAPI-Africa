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
    public interface IPMMasterService : IService<PMMaster>
    {
        IQueryable<PMMaster> GetAllPMMasterList(int id);
    }
    public class PMMasterService : Service<PMMaster>, IPMMasterService
    {
        private readonly IRepositoryAsync<PMMaster> _repository;
        public PMMasterService(IRepositoryAsync<PMMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<PMMaster> GetAllPMMasterList(int brandid)
        {
            return _repository.GetAllPMMasterList(brandid);
        }

        public override void Delete(PMMaster entity)
        {
            entity.Status=MasterStatus.Deleted;
            entity.ObjectState=ObjectState.Modified;
            base.Delete(entity);
        }
    }
}
