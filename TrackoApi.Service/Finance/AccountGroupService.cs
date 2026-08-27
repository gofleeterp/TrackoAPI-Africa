using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IAccountGroupService : IService<AccountGroup>
    {
        IQueryable<AccountGroup> GetAllAccountGroupList(int id);
    }
    public class AccountGroupService : Service<AccountGroup>, IAccountGroupService
    {
        private readonly IRepositoryAsync<AccountGroup> _repository;
        public AccountGroupService(IRepositoryAsync<AccountGroup> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<AccountGroup> GetAllAccountGroupList(int brandid)
        {
            return _repository.GetAllAccountGroupList(brandid);
        }

        public override AccountGroup Insert(AccountGroup entity)
        {
            var parentGroup = this.Queryable().FirstOrDefault(x=>x.Id==entity.ParentGroupId);
            if (parentGroup == null)
            {
                throw new BusinessException(ErrorCode.GLB106,"Parent Group is Required");
            }
            entity.IsRevenue = parentGroup.IsRevenue;
            entity.ObjectState=ObjectState.Added;
            return base.Insert(entity);
        }
    }
}
