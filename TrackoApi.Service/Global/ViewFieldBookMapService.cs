using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;

namespace TrackoApi.Service
{
    public interface IViewFieldBookMapService : IService<ViewFieldBookMap>
    {
    }
    public class ViewFieldBookMapService : Service<ViewFieldBookMap>, IViewFieldBookMapService
    {
        private readonly IRepositoryAsync<ViewFieldBookMap> _repository;
        public ViewFieldBookMapService(IRepositoryAsync<ViewFieldBookMap> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}