using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IRecordAccessLogService : IService<ApiRecordAccessLog>
    {
        
    }
    public class RecordAccessLogService: Service<ApiRecordAccessLog>,IRecordAccessLogService
    {
        private readonly IRepositoryAsync<ApiRecordAccessLog> _repository;
        public RecordAccessLogService(IRepositoryAsync<ApiRecordAccessLog> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
