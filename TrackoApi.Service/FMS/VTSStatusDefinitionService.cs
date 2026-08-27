using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.DTS;

namespace TrackoApi.Service
{
    public interface IDTSStatusService : IService<DTSStatus>
    {
        
    }
    public class DTSStatusService : Service<DTSStatus>, IDTSStatusService
    {
        private readonly IRepositoryAsync<DTSStatus> _repository;
        public DTSStatusService(IRepositoryAsync<DTSStatus> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public override void Delete(DTSStatus entity)
        {
            entity.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
