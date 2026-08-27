using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IMasterAliasService : IService<MasterAlias>
    {
        IQueryable<MasterAlias> GetAllMasterAliasList(int id);
    }
    public class MasterAliasService : Service<MasterAlias>, IMasterAliasService
    {
        private readonly IRepositoryAsync<MasterAlias> _repository;
        public MasterAliasService(IRepositoryAsync<MasterAlias> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<MasterAlias> GetAllMasterAliasList(int brandid)
        {
            return _repository.GetAllMasterAliasList(brandid);
        }
    }
}
