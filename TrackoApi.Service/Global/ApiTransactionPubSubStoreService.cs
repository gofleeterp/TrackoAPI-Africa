using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IApiPubSubStoreService : IService<ApiPubSubStore>
    {
        
    }
    public class ApiPubSubStoreService : Service<ApiPubSubStore>, IApiPubSubStoreService
    {
        
        public ApiPubSubStoreService(IRepositoryAsync<ApiPubSubStore> repository) : base(repository)
        {
        }
    }
}
