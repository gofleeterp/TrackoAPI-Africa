using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
   public static class VoucherRepository
    {
        public static IQueryable<Voucher> GetAllVoucherList(this IRepository<Voucher> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
    }
}
