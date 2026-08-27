using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IObjectClassService : IService<ObjectClass>
    {
        
    }
    public class ObjectClassService : Service<ObjectClass>, IObjectClassService
    {
        private readonly IRepositoryAsync<ObjectClass> _repository;
        public ObjectClassService(IRepositoryAsync<ObjectClass> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(ObjectClass entity)
        {
            var repo = _repository.GetRepository<ObjectClassMap>();
            if (repo.Queryable().Any(x => x.ClassId == entity.Id))
            {
                var all = _repository.Queryable().FirstOrDefault(x => x.ClassName == "All");
                if (all == null)
                {
                    var count = repo.Queryable().Count(x => x.ClassId == entity.Id);
                    throw new BusinessException(ErrorCode.GLB108,
                        $"The Class {entity.ClassName} is Mapped with {count} Objects. Please unmap all objects");
                }
                foreach (var obj in repo.Queryable().Where(x => x.ClassId == entity.Id))
                {
                    obj.ClassId = all.Id;
                    obj.ObjectState=ObjectState.Modified;
                    obj.fk_Class = all;
                }
            }
            base.Delete(entity);
        }
    }
}
