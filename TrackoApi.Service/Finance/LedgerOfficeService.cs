using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;

namespace TrackoApi.Service
{
    public interface ILedgerOfficeService : IService<LedgerOffice>
    {

    }
    public class LedgerOfficeService : Service<LedgerOffice>, ILedgerOfficeService
    {
        private readonly IRepositoryAsync<LedgerOffice> _repository;
        public LedgerOfficeService(IRepositoryAsync<LedgerOffice> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
