using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ISpareMasterService : IService<SpareMaster>
    {
        IQueryable<SpareMaster> GetAllSpareMasterList(int id);
        void AlterStatus(List<long> ids);
    }
    public class SpareMasterService : Service<SpareMaster>, ISpareMasterService
    {
        private readonly IRepositoryAsync<SpareMaster> _repository;
        public SpareMasterService(IRepositoryAsync<SpareMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<SpareMaster> GetAllSpareMasterList(int brandid)
        {
            return _repository.GetAllSpareMasterList(brandid);
        }

        public void AlterStatus(List<long> ids)
        {
            foreach (var record in ids.Select(id => this._repository.Find(id)))
            {
                record.Status = record.Status == MasterStatus.Active ? MasterStatus.Suspended : MasterStatus.Active;
                record.ObjectState = ObjectState.Modified;
            }
        }
    }
}
