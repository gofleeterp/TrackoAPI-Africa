using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface IVTSVendorServiceService : IService<VTSVendorService>
    {
        
    }
    public class VTSVendorServiceService : Service<VTSVendorService>, IVTSVendorServiceService
    {
        private readonly IRepositoryAsync<VTSVendorService> _repository;
        public VTSVendorServiceService(IRepositoryAsync<VTSVendorService> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public override void Delete(VTSVendorService entity)
        {
            entity.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
