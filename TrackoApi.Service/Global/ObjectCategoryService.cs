using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Service
{
    public interface IObjectCategoryService : IService<ObjectCategory>
    {
    }

    public class ObjectCategoryService : Service<ObjectCategory>, IObjectCategoryService
    {
        private readonly IRepositoryAsync<ObjectCategory> _repository;

        public ObjectCategoryService(IRepositoryAsync<ObjectCategory> repository) : base(repository)
        {
            _repository = repository;
        }

        public override ObjectCategory Insert(ObjectCategory entity)
        {
            try
            {
                if (entity.ObjectState != ObjectState.Added) return base.Insert(entity);
                var cls = new ObjectClass()
                {
                    Category = entity,
                    CategoryId = entity.Id,
                    ClassName = "All",
                    IsReserved = true,
                    ObjectState = ObjectState.Added,
                    RoleId = entity.RoleId
                };
                entity.ObjectClasses = new List<ObjectClass>() { cls };
                switch (entity.RoleTypeId)
                {
                    case 1145:
                        var accountsbyroleid =
                            _repository.GetRepository<LedgerRole>()
                                .Queryable()
                                .Where(x => x.RoleId == cls.RoleId && !x.fk_Ledger.IsDefaulter)
                                .Select(x => x.LedgerId).ToList();
                        //(from l in
                        //    _repository.GetRepository<Ledger>()
                        //        .Queryable()
                        //        .Where(x => x.AccountRoleId == cls.RoleId && !x.IsDefaulter)
                        // join r in _repository.GetRepository<LedgerRole>().Queryable() on l.Id equals r.LedgerId
                        // select l.Id);
                        if (accountsbyroleid.Any())
                        {
                            var accountsmap = accountsbyroleid.Select(x => new ObjectClassMap
                            {
                                Id = 0,
                                ObjectState = ObjectState.Added,
                                ObjectId = x,
                                ClassId = cls.Id,
                                CategoryId = entity.Id,
                                fk_Class = cls,
                                fk_Category = entity
                            }).ToList();
                            cls.ObjectMappings = accountsmap;
                        }
                        break;

                    case 1146:
                        var groupids = _repository.UOW.Context.AccountGroupChildren.Where(x => x.GrandParentId == cls.RoleId).Select(x => (long?)x.GroupId).ToArray();
                        var accountsbyGroupId =
                            _repository.GetRepository<Ledger>()
                                .Queryable()
                                .Where(x => x.GroupId!=null&& groupids.Contains(x.GroupId)&& !x.IsDefaulter)
                                .Select(x => x.Id).ToList();
                        if (accountsbyGroupId.Any())
                        {
                            var accountsmap = accountsbyGroupId.Select(x => new ObjectClassMap()
                            {
                                Id = 0,
                                ObjectState = ObjectState.Added,
                                ObjectId = x,
                                ClassId = cls.Id,
                                CategoryId = entity.Id,
                                fk_Class = cls,
                                fk_Category = entity
                            }).ToList();
                            cls.ObjectMappings = accountsmap;
                        }
                        break;

                    case 1292:
                        var offices =
                            _repository.GetRepository<OfficeMaster>()
                                .Queryable().Where(x => x.Status == MasterStatus.Active).Select(x => x.Id).ToList();
                        if (offices.Any())
                        {
                            var officemap = offices.Select(x => new ObjectClassMap()
                            {
                                Id = 0,
                                ObjectState = ObjectState.Added,
                                ObjectId = x,
                                ClassId = cls.Id,
                                CategoryId = entity.Id,
                                fk_Class = cls,
                                fk_Category = entity
                            }).ToList();
                            cls.ObjectMappings = officemap;
                        }
                        break;
                }
                return base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}