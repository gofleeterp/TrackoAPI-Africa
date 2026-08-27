using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service.FMS
{
    public interface IRepairLabourLogService : IService<RepairLabourLog>
    {
        
    }
    public class RepairLabourLogService: Service<RepairLabourLog>, IRepairLabourLogService
    {
        public RepairLabourLogService(IRepositoryAsync<RepairLabourLog> _repository):base(_repository)
        {
            
        }
    }
}
