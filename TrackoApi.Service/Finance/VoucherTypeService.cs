using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IVoucherTypeService : IService<VoucherType>
    {
        
    }
    public class VoucherTypeService: Service<VoucherType>,IVoucherTypeService
    {
        private readonly IRepositoryAsync<VoucherType> _repository;
        public VoucherTypeService(IRepositoryAsync<VoucherType> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
