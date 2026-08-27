using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.ViewModels.Global;

using static System.Collections.Specialized.BitVector32;

namespace TrackoAPI.Repository
{
    public static class TransactionSupportLogRepository
    {
        public static IQueryable<TransactionSupportLog> GetAllTransactionSupportLogList(this IRepository<TransactionSupportLog> repository, long id) => repository.Queryable().Where(x => id == x.Id);
        
    }
}
