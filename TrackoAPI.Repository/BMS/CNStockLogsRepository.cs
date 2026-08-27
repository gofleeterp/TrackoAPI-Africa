using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.BMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoAPI.Repository.BMS
{
    public static class CNStockLogsRepository
    {
        public static IQueryable<vwCNStockSearch> GetTop10CnStock(this IRepositoryAsync<CNStockLog> repo,
            long challanOfficeId, long stockOfficeId, DateTime stockDate, string searchTerm)
        {
            return
                repo.SelectQuery<vwCNStockSearch>(
                    $"SELECT Id,CNId,CNNo,StockDate,StockOfficeId,StockQty FROM [dbo].[Fun_CNStockSearch]({challanOfficeId},{stockOfficeId},'{stockDate.ToString("yyyy-MM-dd HH:mm:ss")}','{searchTerm}')");
        }
    }
}
