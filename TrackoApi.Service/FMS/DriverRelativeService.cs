using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IDriverRelativeService : IService<DriverRelative>
    {
        IQueryable<DriverRelative> GetAllDriverRelativeList(int id);
    }
    public class DriverRelativeService : Service<DriverRelative>, IDriverRelativeService
    {
        private readonly IRepositoryAsync<DriverRelative> _repository;
        public DriverRelativeService(IRepositoryAsync<DriverRelative> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DriverRelative> GetAllDriverRelativeList(int brandid)
        {
            return _repository.GetAllDriverRelativeList(brandid);
        }
    }
}
