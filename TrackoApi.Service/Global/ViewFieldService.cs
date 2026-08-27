using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IViewFieldService : IService<ViewField>
    {
    }
    public class ViewFieldService : Service<ViewField>, IViewFieldService
    {
        private readonly IRepositoryAsync<ViewField> _repository;
        public ViewFieldService(IRepositoryAsync<ViewField> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Update(ViewField ent)
        {
            var db =
                _repository.Queryable().First(x => x.Id == ent.Id);
            if (db!=null&&db.IsReserved)
            {
                if (db.IsRequired != ent.IsRequired || db.IsReserved != ent.IsReserved)
                {
                    throw new BusinessException(ErrorCode.GLB107, "Cannot Modify Reserved Information for Build-In Fields");
                }
            }
            base.Update(ent);
        }
    }
    
}
