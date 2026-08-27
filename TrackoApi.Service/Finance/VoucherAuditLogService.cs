using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IVoucherAuditLogService : IService<VoucherAuditLog>
    {
        
    }
    public class VoucherAuditLogService:Service<VoucherAuditLog>,IVoucherAuditLogService
    {
        private readonly IRepositoryAsync<VoucherAuditLog> _repo;

        public VoucherAuditLogService(IRepositoryAsync<VoucherAuditLog> repository):base(repository)
        {
            _repo = repository;
        }
    }
}
