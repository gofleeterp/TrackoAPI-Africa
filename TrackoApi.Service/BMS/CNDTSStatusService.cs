using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;

namespace TrackoApi.Service.TMS
{
    public interface ICNDTSStatusService : IService<CNDTSStatus>
    {
    }
    public class CNDTSStatusService : Service<CNDTSStatus>, ICNDTSStatusService
    {
        private readonly IRepositoryAsync<CNDTSStatus> _repository;
        public CNDTSStatusService(IRepositoryAsync<CNDTSStatus> repository) : base(repository)
        {
            _repository = repository;
        }

        public override CNDTSStatus Insert(CNDTSStatus entity)
        {
            return base.Insert(entity);
        }

        public override void Update(CNDTSStatus entity)
        {
            base.Update(entity);
        }

        public override void Patch(CNDTSStatus entity)
        {
            base.Patch(entity);
        }
    }
}
