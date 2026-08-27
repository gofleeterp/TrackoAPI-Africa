using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;

namespace TrackoAPI.Repository
{
   public static class PurchaseOrderRepository
    {
        public static IQueryable<PurchaseOrder> GetAllPurchaseOrderList(this IRepository<PurchaseOrder> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
