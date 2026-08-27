using Service.Pattern;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS.Loan;

namespace TrackoApi.Service
{
    public interface ILoanLogService : IService<LoanLog>
    {
      
    }
    public class LoanLogService : Service<LoanLog>, ILoanLogService
    {
        private readonly IRepositoryAsync<LoanLog> _repository;
        public LoanLogService(IRepositoryAsync<LoanLog> repository) : base(repository)
        {
            _repository = repository;
        }

       
    }
}
