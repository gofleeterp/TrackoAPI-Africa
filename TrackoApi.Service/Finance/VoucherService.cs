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
    public interface IVoucherService : IService<Voucher>
    {
        IQueryable<Voucher> GetAllVoucherList(int id);
    }
    public class VoucherService : Service<Voucher>, IVoucherService
    {
        private readonly IRepositoryAsync<Voucher> _repository;
        public VoucherService(IRepositoryAsync<Voucher> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<Voucher> GetAllVoucherList(int brandid)
        {
            return _repository.GetAllVoucherList(brandid);
        }
    }
}
