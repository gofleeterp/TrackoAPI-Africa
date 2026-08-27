using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IDriverGuarantorService : IService<DriverGuarantor>
    {
        IQueryable<DriverGuarantor> GetAllDriverGuarantorList(int id);
    }
    public class DriverGuarantorService : Service<DriverGuarantor>, IDriverGuarantorService
    {
        private readonly IRepositoryAsync<DriverGuarantor> _repository;
        public DriverGuarantorService(IRepositoryAsync<DriverGuarantor> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DriverGuarantor> GetAllDriverGuarantorList(int brandid)
        {
            return _repository.GetAllDriverGuarantorList(brandid);
        }
    }
}
