using Service.Pattern;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS.Loan;

namespace TrackoApi.Service
{
    public interface ILoanVehicleLogService : IService<LoanVehicleLog>
    {
      
    }
    public class LoanVehicleLogService : Service<LoanVehicleLog>, ILoanVehicleLogService
    {
        private readonly IRepositoryAsync<LoanVehicleLog> _repository;
        public LoanVehicleLogService(IRepositoryAsync<LoanVehicleLog> repository) : base(repository)
        {
            _repository = repository;
        }

       
    }
}
