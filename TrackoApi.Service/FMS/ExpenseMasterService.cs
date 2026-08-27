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
    public interface IExpenseMasterService : IService<ExpenseMaster>
    {
        IQueryable<ExpenseMaster> GetAllExpenseMasterList(int id);

        void AlterStatus(List<long> ids);
    }
    public class ExpenseMasterService : Service<ExpenseMaster>, IExpenseMasterService
    {
        private readonly IRepositoryAsync<ExpenseMaster> _repository;
        public ExpenseMasterService(IRepositoryAsync<ExpenseMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<ExpenseMaster> GetAllExpenseMasterList(int brandid)
        {
            return _repository.GetAllExpenseMasterList(brandid);
        }

        public void AlterStatus(List<long> ids)
        {
            foreach (var id in ids)
            {
                var record=this._repository.Find(id);
                record.Status = record.Status == MasterStatus.Active ? MasterStatus.Suspended : MasterStatus.Active;
                record.ObjectState=ObjectState.Modified;
            }
        }
    }
}
