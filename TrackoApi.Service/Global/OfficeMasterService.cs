using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IOfficeMasterService : IService<OfficeMaster>
    {
        Task MapOfficeToDefaultClass(OfficeMaster officeMaster);
    }
    public class OfficeMasterService : Service<OfficeMaster>, IOfficeMasterService
    {
        private readonly IRepositoryAsync<OfficeMaster> _repository;
        public OfficeMasterService(IRepositoryAsync<OfficeMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        //public override OfficeMaster Insert(OfficeMaster entity)
        //{
        //    var clsRepo = _repository.GetRepository<ObjectClassMap>();
        //    var cls = _repository.GetRepository<ObjectClass>().Queryable()
        //             .Where(x => x.ClassName == "All" && x.RoleId == 1292 && x.Category.RoleTypeId== 1292).Select(x => new
        //             {
        //                 ClassId = x.Id,
        //                 x.CategoryId
        //             }).ToList();
        //    var list = cls.Select(x => new ObjectClassMap
        //    {
        //        Id = 0,
        //        ObjectState = ObjectState.Added,
        //        ObjectId = entity.Id,
        //        ClassId = x.ClassId,
        //        CategoryId = x.CategoryId
        //    }).ToList();
        //    clsRepo.InsertRange(list);
        //    return base.Insert(entity);
        //}
        public async Task MapOfficeToDefaultClass(OfficeMaster officeMaster)
        {
            var clsRepo = _repository.GetRepository<ObjectClassMap>();
            var cls = await _repository.GetRepository<ObjectClass>().Queryable()
                .Where(x => x.ClassName == "All" && x.RoleId == 1292 && x.Category.RoleTypeId == 1292 && x.ObjectMappings.All(y => y.ObjectId != officeMaster.Id))
                .Select(x => new
                {
                    ClassId = x.Id,
                    x.CategoryId
                }).ToListAsync();
            if (cls.Any())
            {
                var list = cls.Select(x => new ObjectClassMap
                {
                    Id = 0,
                    ObjectState = ObjectState.Added,
                    ObjectId = officeMaster.Id,
                    ClassId = x.ClassId,
                    CategoryId = x.CategoryId
                }).ToList();
                clsRepo.InsertRange(list);
                await _repository.UOW.SaveChangesAsync();
            }
        }
    }
    
}
