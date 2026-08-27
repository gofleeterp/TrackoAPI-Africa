using Service.Pattern;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS.Loan;

namespace TrackoApi.Service
{
    public interface ILoanService : IService<Loan>
    {
      
    }
    public class LoanService : Service<Loan>, ILoanService
    {
        private readonly IRepositoryAsync<Loan> _repository;
        public LoanService(IRepositoryAsync<Loan> repository) : base(repository)
        {
            _repository = repository;
        }

       
    }
}
