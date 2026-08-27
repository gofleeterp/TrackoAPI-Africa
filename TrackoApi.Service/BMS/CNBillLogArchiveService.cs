using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;

namespace TrackoApi.Service
{
    public interface ICNBillLogArchiveService : IService<CNBillLogArchive>
    {

    }
    public class CNBillLogArchiveService : Service<CNBillLogArchive>, ICNBillLogArchiveService
    {
        private readonly IRepositoryAsync<CNBillLogArchive> _repository;
        public CNBillLogArchiveService(IRepositoryAsync<CNBillLogArchive> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
