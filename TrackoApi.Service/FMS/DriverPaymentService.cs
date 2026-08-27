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
    public interface IDriverPaymentService : IService<DriverPayment>
    {
        IQueryable<DriverPayment> GetAllDriverPaymentList(int id);
    }
    public class DriverPaymentService : Service<DriverPayment>, IDriverPaymentService
    {
        private readonly IRepositoryAsync<DriverPayment> _repository;
        public DriverPaymentService(IRepositoryAsync<DriverPayment> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DriverPayment> GetAllDriverPaymentList(int brandid)
        {
            return _repository.GetAllDriverPaymentList(brandid);
        }
    }
}
