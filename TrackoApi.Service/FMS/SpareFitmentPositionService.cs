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
    public interface ISpareFitmentPositionService : IService<SpareFitmentPosition>
    {
        IQueryable<SpareFitmentPosition> GetAllSpareFitmentPositionList(int id);
    }
    public class SpareFitmentPositionService : Service<SpareFitmentPosition>, ISpareFitmentPositionService
    {
        private readonly IRepositoryAsync<SpareFitmentPosition> _repository;
        public SpareFitmentPositionService(IRepositoryAsync<SpareFitmentPosition> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<SpareFitmentPosition> GetAllSpareFitmentPositionList(int brandid)
        {
            return _repository.GetAllSpareFitmentPositionList(brandid);
        }
    }
}
