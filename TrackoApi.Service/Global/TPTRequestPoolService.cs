using Service.Pattern;
using TrackoApi.Models.Global;
using System.Linq;
using Repository.Pattern.Core.UnitOfWork;
using Repository.Pattern.Core.Repositories;
using System;
using TrackoAPI.Repository;
using TrackoApi.Models.BMS;
namespace TrackoApi.Service.Global
{
    public interface ITPTRequestPoolService : IService<TPTRequestPool>
    {
        IQueryable<TPTRequestPool> GetAllTPTRequestPoolList(int id);
    }
    public class TPTRequestPoolService : Service<TPTRequestPool>, ITPTRequestPoolService
    {
        private readonly IRepositoryAsync<TPTRequestPool> _repository;
        public TPTRequestPoolService(IRepositoryAsync<TPTRequestPool> repository) : base(repository)
        {
            _repository = repository;
        }
        public override TPTRequestPool Insert(TPTRequestPool entity)
        {
            return base.Insert(entity);
        }
        public override void Update(TPTRequestPool entity)
        {
            base.Update(entity);
        }

        public IQueryable<TPTRequestPool> GetAllTPTRequestPoolList(int id)
        {
            return _repository.GetAllTPTRequestPoolList(id);
        }
    }
}
