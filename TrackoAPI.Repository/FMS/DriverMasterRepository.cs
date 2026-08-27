using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Reports.ViewModels.FMS.Driver;
using TrackoAPI.Reports.ViewModels.FMS.Repair;

namespace TrackoAPI.Repository
{
    public static class DriverMasterRepository
    {
        public static IQueryable<DriverMaster> GetAllDriverMasterList(this IRepository<DriverMaster> repository, long id)
            => repository.Queryable().Where(x => id == x.Id);

        public static IQueryable<VwDriverAccountSummary> GetAllDriverAccountSummary(
            this IRepository<DriverMaster> repository,
             DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var dvrdata = repository.Queryable();
            var vdrepo = repository.GetRepository<VoucherDetail>().Queryable();
            var tripsrepo = repository.GetRepository<VehicleTripSettlement>().Queryable();
            var advrepo = repository.GetRepository<TripAdvanceLog>().Queryable();

            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectsForReport(classIds, categoryId, accountIds);

            var data = from d in dvrdata.Where(x => objectids.Item2.Contains(x.Id))
                join v in vdrepo.Where(x => x.Voucher.VoucherDate < FromDate && x.Voucher.IsAccepted) on d.Id equals
                    v.AccountId into o
                join v in
                    vdrepo.Where(
                        x =>
                            x.Voucher.VoucherDate >= FromDate && x.Voucher.VoucherDate <= ToDate && x.Voucher.IsAccepted)
                    on d.Id equals
                    v.AccountId into vd
                join tr in
                    tripsrepo.Where(x => x.StartDate >= FromDate && x.StartDate <= ToDate && x.SettleDate != null) on
                    d.Id equals tr.Driver1Id into g2
                join utr in
                    advrepo.Where(x => x.AdvanceDate >= FromDate && x.AdvanceDate <= ToDate && x.SettlementId == null) on
                    d.Id equals utr.DriverId into g3
                select new VwDriverAccountSummary
                {
                    DriverId = d.Id,
                    DriverName = d.DriverName,
                    DriverCode = d.DriverCode,
                    OpeningBalance = o.Sum(x => (decimal?) x.Amount),
                    DebitAmount = vd.Where(x => x.Amount > 0).Sum(x => (decimal?) x.Amount),
                    CreditAmount = vd.Where(x => x.Amount < 0).Sum(x => (decimal?) x.Amount),
                    TripAdvAmount = g2.Sum(x => x.TripAdvanceAmt),
                    TripExpAmount = g2.Sum(x=>x.TripExpenseAmt),
                    UnSettledAdvAmount = g3.Sum(x => x.CashAmount)
                };
            return data;


        }

         
            public static IQueryable<vwDriverTripPerformanceSummary> GetAllDriverTripPerformanceSummary(
            this IRepository<DriverMaster> repository,
             DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var vehdata = repository.Queryable();
            var tripsrepo = repository.GetRepository<VehicleTripSettlement>().Queryable();
           

            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data = from v in vehdata
                       join o in objectids on v.Id equals o.ObjectId
                       join tr in
                           tripsrepo.Where(x => x.StartDate >= FromDate && x.StartDate <= ToDate && x.SettleDate != null) on
                           v.Id equals tr.VehicleId into g1
                       
                       select new vwDriverTripPerformanceSummary
                       {
                           CategoryName = o.fk_Category.CategoryName,
                           ClassName = o.fk_Class.ClassName,
                           DriverId = v.Id,
                           DriverName = v.DriverName,
                           TSCount = g1.Count(),
                           TLCount = g1.Select(x => x.TripLogs.Select(z => z.Id)).Count(),
                           Days = g1.Sum(x => DbFunctions.DiffDays(x.StartDate, x.EndDate) + 1),
                           TotKmRun = g1.Sum(x => x.TotalKmRun),
                           TotFreight = g1.Sum(x => x.BookingFreight),
                           TotAdv = g1.Sum(x => x.TripAdvanceAmt),
                           TotExp = g1.Sum(x => x.TripExpenseAmt),

                           FNet = g1.Sum(x => x.BookingFreight - x.TripExpenseAmt),

                           TotFuelExp = g1.Sum(x => x.FuelExpenseAmt),

                           TotBdgtFuelQty = g1.Sum(x => x.TotalBudgetedFuelQty),
                           TotActualFuelQty = g1.Sum(x => x.ActualQty),
                           TotExtraFuelQty = g1.Sum(x => x.ExtraFuelQty)
                       };
            return data;
        }
    }

}
