using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVoucherDetailReferenceService : IService<VoucherDetailReference>
    {
        IQueryable<VoucherDetailReference> GetAllVoucherDetailReferenceList(int id);
        IQueryable<VoucherDetailReference> GetUnPaidReferences();
    }
    public class VoucherDetailReferenceService : Service<VoucherDetailReference>, IVoucherDetailReferenceService
    {
        private readonly IRepositoryAsync<VoucherDetailReference> _repository;
        public VoucherDetailReferenceService(IRepositoryAsync<VoucherDetailReference> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VoucherDetailReference> GetAllVoucherDetailReferenceList(int brandid)
        {
            return _repository.GetAllVoucherDetailReferenceList(brandid);
        }

        public IQueryable<VoucherDetailReference> GetUnPaidReferences()
        {
            return _repository.Queryable().Where(x => x.RefId == null&&(x.AgainstReferences.Count() == 0 || (Math.Abs(x.Amount) - (Math.Abs(x.AgainstReferences.Sum(y => (decimal?)y.Amount) ?? 0))) > 0));
            //return  from s in _repository.Queryable()
            //    join l in _repository.Queryable() on s.Id equals l.RefId into refs
            //        //where (refs.Any()&&s.Amount-refs.Sum(x=>x.Amount)>0)||!refs.Any()
            //        where (Math.Abs(s.Amount) - Math.Abs(refs.Sum(x => x.Amount))) > 0
            //        select s;
        }
        
    }
}
