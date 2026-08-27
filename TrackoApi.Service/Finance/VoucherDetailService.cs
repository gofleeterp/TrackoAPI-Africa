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
    public interface IVoucherDetailService : IService<VoucherDetail>
    {
        IQueryable<VoucherDetail> GetAllVoucherDetailList(int id);
    }
    public class VoucherDetailService : Service<VoucherDetail>, IVoucherDetailService
    {
        private readonly IRepositoryAsync<VoucherDetail> _repository;
        public VoucherDetailService(IRepositoryAsync<VoucherDetail> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VoucherDetail> GetAllVoucherDetailList(int brandid)
        {
            return _repository.GetAllVoucherDetailList(brandid);
        }
    }
}
