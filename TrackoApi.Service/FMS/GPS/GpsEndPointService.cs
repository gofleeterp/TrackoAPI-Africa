using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;

namespace TrackoApi.Service.FMS.GPS
{
    public interface IGpsEndPointService : IService<GpsEndPoint>
    {
    }
    public class GpsEndPointService: Service<GpsEndPoint>, IGpsEndPointService
    {
        private readonly IRepositoryAsync<GpsEndPoint> _repository;
        public GpsEndPointService(IRepositoryAsync<GpsEndPoint> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
