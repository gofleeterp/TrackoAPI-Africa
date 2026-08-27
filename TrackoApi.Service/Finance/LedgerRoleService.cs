using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;

namespace TrackoApi.Service
{
    public interface ILedgerRoleService : IService<LedgerRole>
    {

    }
    public class LedgerRoleService : Service<LedgerRole>, ILedgerRoleService
    {
        private readonly IRepositoryAsync<LedgerRole> _repository;
        public LedgerRoleService(IRepositoryAsync<LedgerRole> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
