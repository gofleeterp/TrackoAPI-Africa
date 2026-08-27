using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IPurchaseOrderService : IService<PurchaseOrder>
    {
        IQueryable<PurchaseOrder> GetAllPurchaseOrderList(int id);
    }
    public class PurchaseOrderService : Service<PurchaseOrder>, IPurchaseOrderService
    {
        private readonly IRepositoryAsync<PurchaseOrder> _repository;
        public PurchaseOrderService(IRepositoryAsync<PurchaseOrder> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<PurchaseOrder> GetAllPurchaseOrderList(int brandid)
        {
            return _repository.GetAllPurchaseOrderList(brandid);
        }
    }
}
