using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.FMS.Battery;
using TrackoAPI.ViewModels.FMS.Tyres;

namespace TrackoAPI.Repository
{
   public static class BatteryMasterRepository
    {
        
    }

    public static class BatteryLifePerfRepository
    {
        private static List<BatteryLifePerformanceLog> UpdatePerformanceLog(this IRepository<BatteryLifePerformanceLog> repository,
            List<vwBatteryPerformanceAgeUpdate> Battery)
        {
            if (Battery.Any())
            {
                var list = Battery.Select(x => $"{x.BatteryId}-{x.Life}").ToList();
                
                var maincall = repository.Queryable().Where(x => list.Contains($"{x.BatteryId}-{x.Life}"));
                if (maincall.Any())
                {
                    var data = maincall.ToList();
                    foreach (var log in data)
                    {
                        var record = Battery.Find(x => x.BatteryId == log.BatteryId);
                        if (record == null) continue;
                        if (record.CurrentAge > 0) log.CurrentAge = record.CurrentAge;
                        if (record.LifeAge > 0) log.LifeAge = record.LifeAge;
                        if (record.PreviousAge > 0) log.PreviousAge = record.PreviousAge;
                        if ((record.CurrentAge + record.LifeAge + record.PreviousAge) <= 0) continue;
                        log.ObjectState=ObjectState.Modified;
                        repository.Update(log);
                    }
                    return data;
                }
                
            }
            return new List<BatteryLifePerformanceLog>();
        }

        public static List<BatteryLifePerformanceLog> UpdatePerformanceLog(
            this IRepository<BatteryLifePerformanceLog> repository,
            long vehicleid,DateTime fromDate,DateTime toDate)
        {
            var setting = repository.GetRepository<ApiConfiguration>().Find("BatteryKMCalculationSource");
            if(setting==null)throw new BusinessException(ErrorCode.GLB103,"Configuration for Battery Calculation Source is Not Defined..");
            
            switch (setting.Value)
            {
                case "0":
                   
                        var Batterylogs=repository.GetRepository<BatteryLog>()
                            .Queryable()
                            .Where(x => x.VoucherTypeId== 35&&x.DocDate.Date>=fromDate.Date&&x.DocDate.Date<=toDate.Date&&x.VehicleId==vehicleid)
                            .Select(x => new {x.BatteryAge,x.BatteryId,x.BatteryLife}).ToList();
                   

                    break;
                case "1":
                    break;
                case "2":
                    break;
                case "3":
                    break;
            }
            return new List<BatteryLifePerformanceLog>();
        }

    }

}
