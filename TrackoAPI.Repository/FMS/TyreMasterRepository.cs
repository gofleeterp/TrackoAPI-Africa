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
using TrackoAPI.ViewModels.FMS.Tyres;

namespace TrackoAPI.Repository
{
   public static class TyreMasterRepository
    {
        public static IQueryable<TyreMaster> GetAllTyreMasterList(this IRepository<TyreMaster> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
    }

    public static class TyreLifePerfRepository
    {
        private static List<TyreLifePerformanceLog> UpdatePerformanceLog(this IRepository<TyreLifePerformanceLog> repository,
            List<vwTyrePerformanceKmUpdate> tyres)
        {
            if (tyres.Any())
            {
                var list = tyres.Select(x => $"{x.TyreId}-{x.Life}").ToList();
                
                var maincall = repository.Queryable().Where(x => list.Contains($"{x.TyreId}-{x.Life}"));
                if (maincall.Any())
                {
                    var data = maincall.ToList();
                    foreach (var log in data)
                    {
                        var record = tyres.Find(x => x.TyreId == log.TyreId);
                        if (record == null) continue;
                        if (record.CurrentMilage > 0) log.CurrentMileage = record.CurrentMilage;
                        if (record.LifeMilage > 0) log.TyreLifeMileage = record.LifeMilage;
                        if (record.PreviousMilage > 0) log.TyrePreviousMileage = record.PreviousMilage;
                        if ((record.CurrentMilage + record.LifeMilage + record.PreviousMilage) <= 0) continue;
                        log.ObjectState=ObjectState.Modified;
                        repository.Update(log);
                    }
                    return data;
                }
                
            }
            return new List<TyreLifePerformanceLog>();
        }

        public static List<TyreLifePerformanceLog> UpdatePerformanceLog(
            this IRepository<TyreLifePerformanceLog> repository,
            long vehicleid,DateTime fromDate,DateTime toDate)
        {
            var setting = repository.GetRepository<ApiConfiguration>().Find("DefaultKmSourceForReporting");
            if(setting==null)throw new BusinessException(ErrorCode.GLB103,"Configuration for Tyre Calculation Source is Not Defined..");
            
            switch (setting.Value)
            {
                case "0":
                   
                        var tyrelogs=repository.GetRepository<TyreLog>()
                            .Queryable()
                            .Where(x => x.VoucherTypeId== 35&&x.VoucherDate.Date>=fromDate.Date&&x.VoucherDate.Date<=toDate.Date&&x.VehicleId==vehicleid)
                            .Select(x => new {x.KmRun,x.TyreId,x.TyreLife}).ToList();
                   

                    break;
                case "1":
                    break;
                case "2":
                    break;
                case "3":
                    break;
            }
            return new List<TyreLifePerformanceLog>();
        }

    }

}
