using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Reports.ViewModels.FMS.Repair;
using TrackoAPI.Reports.ViewModels.FMS.Tyre;
using TrackoAPI.Reports.ViewModels.FMS.Driver;
using TrackoAPI.Reports.ViewModels.FMS.Global;

namespace TrackoAPI.Repository
{
    public static class VehicleMasterRepository
    {
        public static IQueryable<VehicleMaster> GetAllVehicleMasterList(this IRepository<VehicleMaster> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }

        public static IQueryable<vwCategoryClassMap> GetAllCategoryClassMap(
            this IRepository<VehicleMaster> repository,
            string classIds, string accountIds, long categoryId)
        {
            var data = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds)
                .Select(x => new vwCategoryClassMap
                {
                    CategoryName = x.fk_Category.CategoryName,
                    ClassName = x.fk_Class.ClassName,
                    ObjectName = x.ObjectName,
                    CategoryTypeId = x.fk_Category.CategoryTypeId,
                    RoleTypeId = x.fk_Category.RoleTypeId,
                    RoleId = x.fk_Category.RoleId,
                    CategoryId = x.CategoryId,
                    ClassId = x.ClassId,
                    ObjectId = x.ObjectId
                });
            return data;

        }

        public static IQueryable<vwVehiclePerformanceSummary> GetAllVehiclePerformanceSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var vehdata = repository.Queryable();
            var tripsrepo = repository.GetRepository<VehicleTripSettlement>().Queryable();
            var tyrerepo = repository.GetRepository<TyreLog>().Queryable();
            var repairrepo = repository.GetRepository<SpareLog>().Queryable();
            var duesrepo = repository.GetRepository<DueTransactionLog>().Queryable();
            var driverrepo = repository.GetRepository<TripAdvanceLog>().Queryable();

            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data = from v in vehdata
                join o in objectids on v.Id equals o.ObjectId
                join tr in
                    tripsrepo.Where(x => x.StartDate >= FromDate && x.StartDate <= ToDate && x.SettleDate != null) on
                    v.Id equals tr.VehicleId into g1
                join ty in
                    tyrerepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate && x.TyreStatusId == 1103 &&
                            x.VoucherTypeId != 42) on
                    v.Id equals ty.VehicleId into g2
                join rp in
                    repairrepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate &&
                            (x.VoucherTypeId == 22 || x.VoucherTypeId == 24)) on
                    v.Id equals rp.VehicleId into g3

                join jq in
                    duesrepo.Where(x => x.PaidDate >= FromDate && x.PaidDate <= ToDate) on
                    v.Id equals jq.VehicleId into g4

                join jq in
                    driverrepo.Where(
                        x =>
                            x.AdvanceDate >= FromDate && x.AdvanceDate <= ToDate &&
                            (x.AdvanceTypeId == 11 || x.AdvanceTypeId == 13
                             || x.AdvanceTypeId == 59 || x.AdvanceTypeId == 14 || x.AdvanceTypeId == 15 ||
                             x.AdvanceTypeId == 16 || x.AdvanceTypeId == 17)) on
                    v.Id equals jq.VehicleId into g5


                select new vwVehiclePerformanceSummary
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    TSCount = g1.Count(),
                    TLCount = g1.Select(x => x.TripLogs.Select(z => z.Id)).Count(),
                    Days = g1.Sum(x => DbFunctions.DiffDays(x.StartDate, x.EndDate) + 1),
                    TotKmRun = g1.Sum(x => x.TotalKmRun),
                    TotFreight = g1.Sum(x => x.BookingFreight),
                    TotExp = g1.Sum(x => x.TripExpenseAmt),

                    cNet = g1.Sum(x => x.BookingFreight - x.TripExpenseAmt),

                    TotTyreExp = g2.Sum(x => x.NetAmount),

                    TotRepairExp = g3.Sum(x => x.NetAmount),
                    TotDuesExp = g4.Sum(x => x.DueAmount),
                    TotDriverExp =
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                    TotGenExp = 0,

                    TotEMIExp = 0,
                    FNet =
                        g1.Sum(x => x.BookingFreight - x.TripExpenseAmt) - g2.Sum(x => x.NetAmount) -
                        g3.Sum(x => x.NetAmount) - g4.Sum(x => x.DueAmount) -
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount))
                };
            return data;
        }

        public static IQueryable<vwVehicleMonthlyPerformanceSummary> GetAllVehicleMonthlyPerformanceSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var vehdata = repository.Queryable();
            var tripsrepo = repository.GetRepository<VehicleTripSettlement>().Queryable();
            var tyrerepo = repository.GetRepository<TyreLog>().Queryable();
            var repairrepo = repository.GetRepository<SpareLog>().Queryable();
            var duesrepo = repository.GetRepository<DueTransactionLog>().Queryable();
            var driverrepo = repository.GetRepository<TripAdvanceLog>().Queryable();

            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data = from v in (from v in vehdata
                join o in objectids on v.Id equals o.ObjectId
                join tr in
                    tripsrepo.Where(x => x.StartDate >= FromDate && x.StartDate <= ToDate && x.SettleDate != null) on
                    v.Id equals tr.VehicleId
                group tr by
                    new
                    {
                        o.fk_Category.CategoryName,
                        o.fk_Class.ClassName,
                        tr.VehicleId,
                        tr.fk_Vehicle.VehicleNo,
                        TripYear = tr.StartDate.Year,
                        TripMonth = tr.StartDate.Month
                    }
                into g1

                select g1)

                join ty in
                    tyrerepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate && x.TyreStatusId == 1103 &&
                            x.VoucherTypeId != 42) on
                    v.Key.VehicleId equals ty.VehicleId into g2
                join rp in
                    repairrepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate &&
                            (x.VoucherTypeId == 22 || x.VoucherTypeId == 24)) on
                    v.Key.VehicleId equals rp.VehicleId into g3

                join jq in
                    duesrepo.Where(x => x.PaidDate >= FromDate && x.PaidDate <= ToDate) on
                    v.Key.VehicleId equals jq.VehicleId into g4

                join jq in
                    driverrepo.Where(
                        x =>
                            x.AdvanceDate >= FromDate && x.AdvanceDate <= ToDate &&
                            (x.AdvanceTypeId == 11 || x.AdvanceTypeId == 13
                             || x.AdvanceTypeId == 59 || x.AdvanceTypeId == 14 || x.AdvanceTypeId == 15 ||
                             x.AdvanceTypeId == 16 || x.AdvanceTypeId == 17)) on
                    v.Key.VehicleId equals jq.VehicleId into g5


                select new vwVehicleMonthlyPerformanceSummary
                {
                    CategoryName = v.Key.CategoryName,
                    ClassName = v.Key.ClassName,
                    VehicleId = v.Key.VehicleId,
                    VehicleNo = v.Key.VehicleNo,
                    TripYear = v.Key.TripYear,
                    TripMonth = v.Key.TripMonth,
                    TSCount = v.Count(),
                    TLCount = v.Select(x => x.TripLogs.Select(z => z.Id)).Count(),
                    Days = v.Sum(x => DbFunctions.DiffDays(x.StartDate, x.EndDate) + 1),
                    TotKmRun = v.Sum(x => x.TotalKmRun),
                    TotFreight = v.Sum(x => x.BookingFreight),
                    TotExp = v.Sum(x => x.TripExpenseAmt),

                    cNet = v.Sum(x => x.BookingFreight - x.TripExpenseAmt),

                    TotTyreExp = g2.Sum(x => x.NetAmount),

                    TotRepairExp = g3.Sum(x => x.NetAmount),
                    TotDuesExp = g4.Sum(x => x.DueAmount),
                    TotDriverExp =
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                    TotGenExp = 0,

                    TotEMIExp = 0,
                    FNet =
                        v.Sum(x => x.BookingFreight - x.TripExpenseAmt) - g2.Sum(x => x.NetAmount) -
                        g3.Sum(x => x.NetAmount) - g4.Sum(x => x.DueAmount) -
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount))
                };
            return data;
        }

        public static IQueryable<vwVehicleTripMileageMatrix> GetAllVehicleTripMileageMatrix(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId, long kmsource)
        {
            //Triplog: kmsource=1, Jobsheet:kmsource=2
            var vehrepo = repository.Queryable();
            var tripsrepo = repository.GetRepository<VehicleMovementLog>().Queryable();
            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var query =
                from v in vehrepo
                join t in
                    tripsrepo.Where(x => (x.TripStartDate >= FromDate && x.TripStartDate <= ToDate) && !x.IsHired &&
                                         (((x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null)) && kmsource == 1) ||
                                          (x.TripTypeId == 1159 && kmsource == 2))) on v.Id equals t.VehicleId
                join o in objectids on v.Id equals o.ObjectId
                select new
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    TripYear = t.TripStartDate.Year,
                    TripMonth = t.TripStartDate.Month,
                    TripDays = DbFunctions.DiffDays(t.TripStartDate, t.UnloadingDate) + 1,
                    TotKmRun = t.TotalKmRun
                };

            var data = from x in query
                group x by new {x.CategoryName, x.ClassName, x.VehicleId, x.VehicleNo, x.TripYear}
                into g
                select new vwVehicleTripMileageMatrix
                {
                    CategoryName = g.Key.CategoryName,
                    ClassName = g.Key.ClassName,
                    VehicleId = g.Key.VehicleId,
                    VehicleNo = g.Key.VehicleNo,
                    TripYear = g.Key.TripYear,
                    DJan = g.Where(x => x.TripMonth == 1).Sum(x => x.TripDays),
                    KJan = g.Where(x => x.TripMonth == 1).Sum(x => x.TotKmRun),

                    DFeb = g.Where(x => x.TripMonth == 2).Sum(x => x.TripDays),
                    KFeb = g.Where(x => x.TripMonth == 2).Sum(x => x.TotKmRun),

                    DMar = g.Where(x => x.TripMonth == 3).Sum(x => x.TripDays),
                    KMar = g.Where(x => x.TripMonth == 3).Sum(x => x.TotKmRun),

                    DApr = g.Where(x => x.TripMonth == 4).Sum(x => x.TripDays),
                    KApr = g.Where(x => x.TripMonth == 4).Sum(x => x.TotKmRun),

                    DMay = g.Where(x => x.TripMonth == 5).Sum(x => x.TripDays),
                    KMay = g.Where(x => x.TripMonth == 5).Sum(x => x.TotKmRun),

                    DJun = g.Where(x => x.TripMonth == 6).Sum(x => x.TripDays),
                    KJun = g.Where(x => x.TripMonth == 6).Sum(x => x.TotKmRun),

                    DJul = g.Where(x => x.TripMonth == 7).Sum(x => x.TripDays),
                    KJul = g.Where(x => x.TripMonth == 7).Sum(x => x.TotKmRun),

                    DAug = g.Where(x => x.TripMonth == 8).Sum(x => x.TripDays),
                    KAug = g.Where(x => x.TripMonth == 8).Sum(x => x.TotKmRun),

                    DSep = g.Where(x => x.TripMonth == 9).Sum(x => x.TripDays),
                    KSep = g.Where(x => x.TripMonth == 9).Sum(x => x.TotKmRun),

                    DOct = g.Where(x => x.TripMonth == 10).Sum(x => x.TripDays),
                    KOct = g.Where(x => x.TripMonth == 10).Sum(x => x.TotKmRun),

                    DNov = g.Where(x => x.TripMonth == 11).Sum(x => x.TripDays),
                    KNov = g.Where(x => x.TripMonth == 11).Sum(x => x.TotKmRun),

                    DDec = g.Where(x => x.TripMonth == 12).Sum(x => x.TripDays),
                    KDec = g.Where(x => x.TripMonth == 12).Sum(x => x.TotKmRun),

                    Dtotal = g.Sum(x => x.TripDays),
                    KTotal = g.Sum(x => x.TotKmRun)



                };
            return data;
        }

        public static IQueryable<vwVehicleJobgroupRepairSummary> GetAllVehicleJobgroupRepairSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId, long kmsource)
        {
            //Triplog: kmsource=1, Jobsheet:kmsource=2
            var vehrepo = repository.Queryable();
            var spareepo = repository.GetRepository<SpareLog>().Queryable();
            var tripsrepo = repository.GetRepository<VehicleMovementLog>().Queryable();
            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var query =
                from v in vehrepo
                join t in
                    spareepo.Where(x => x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) on v.Id equals t.VehicleId
                join o in objectids on v.Id equals o.ObjectId
                select new
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    VehicleType = v.fk_VehicleType == null ? null : v.fk_VehicleType.Name,
                    TripYear = t.VoucherDate.Year,
                    SpareCatName = t.fk_Spare.fk_SpareGroup == null ? null : t.fk_Spare.fk_SpareGroup.Name,
                    Amount = t.NetAmount
                };
            var km = from x in
                tripsrepo.Where(
                    x =>
                        (x.TripStartDate >= FromDate && x.TripStartDate <= ToDate) && !x.IsHired &&
                        (((x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null)) && kmsource == 1) || (x.TripTypeId == 1159 && kmsource == 2)))
                group x by new {x.VehicleId, x.TripStartDate.Year}
                into g1
                select new
                {
                    VehicleId = g1.Key.VehicleId.Value,
                    TripYear = g1.Key.Year,
                    TotalKm = g1.Sum(x => x.TotalKmRun)
                };

            var data = from d in (from x in query

                group x by new {x.CategoryName, x.ClassName, x.VehicleId, x.VehicleNo, x.VehicleType, x.TripYear}
                into g
                select g)
                join l in km on new {d.Key.VehicleId, d.Key.TripYear} equals new {l.VehicleId, l.TripYear} into g1
                select new vwVehicleJobgroupRepairSummary
                {
                    VehicleId = d.Key.VehicleId,
                    VehicleType = d.Key.VehicleType,
                    CategoryName = d.Key.CategoryName,
                    ClassName = d.Key.ClassName,
                    VehicleNo = d.Key.VehicleNo,
                    TripYear = d.Key.TripYear,
                    Lubes = d.Where(x => x.SpareCatName == "Lubes").Sum(x => x.Amount),
                    Body = d.Where(x => x.SpareCatName == "Body").Sum(x => x.Amount),

                    Engine = d.Where(x => x.SpareCatName == "Engine").Sum(x => x.Amount),
                    Gear = d.Where(x => x.SpareCatName == "Gear").Sum(x => x.Amount),

                    General = d.Where(x => x.SpareCatName == "General").Sum(x => x.Amount),
                    Electrical = d.Where(x => x.SpareCatName == "Electrical").Sum(x => x.Amount),

                    Clutch = d.Where(x => x.SpareCatName == "Clutch").Sum(x => x.Amount),
                    Hub = d.Where(x => x.SpareCatName == "Hub").Sum(x => x.Amount),

                    Brake = d.Where(x => x.SpareCatName == "Brake").Sum(x => x.Amount),
                    Kamani = d.Where(x => x.SpareCatName == "Kamani").Sum(x => x.Amount),

                    Pump = d.Where(x => x.SpareCatName == "Pump").Sum(x => x.Amount),
                    Accessory = d.Where(x => x.SpareCatName == "Accessory").Sum(x => x.Amount),

                    Crown = d.Where(x => x.SpareCatName == "Crown").Sum(x => x.Amount),
                    CenterJoint = d.Where(x => x.SpareCatName == "CenterJoint").Sum(x => x.Amount),

                    Cooling = d.Where(x => x.SpareCatName == "Cooling").Sum(x => x.Amount),
                    Steering = d.Where(x => x.SpareCatName == "Steering").Sum(x => x.Amount),

                    Others = d.Where(x => x.SpareCatName == null).Sum(x => x.Amount),
                    TotalAmount = d.Sum(x => x.Amount),

                    TotalKm = g1.Sum(x => x.TotalKm)

                };
            return data;
        }

        public static IQueryable<vwVehicleMonthRepairSummary> GetAllVehicleMonthRepairSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId, long kmsource)
        {
            //Triplog: kmsource=1, Jobsheet:kmsource=2
            var vehrepo = repository.Queryable();
            var spareepo = repository.GetRepository<SpareLog>().Queryable();
            var tripsrepo = repository.GetRepository<VehicleMovementLog>().Queryable();
            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var query =
                from v in vehrepo
                join t in
                    spareepo.Where(x => x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) on v.Id equals t.VehicleId
                join o in objectids on v.Id equals o.ObjectId
                select new
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    VehicleType = v.fk_VehicleType == null ? null : v.fk_VehicleType.Name,
                    TripYear = t.VoucherDate.Year,
                    TripMonth = t.VoucherDate.Month,
                    Amount = t.NetAmount
                };
            var km = from x in
                tripsrepo.Where(
                    x =>
                        (x.TripStartDate >= FromDate && x.TripStartDate <= ToDate) && !x.IsHired &&
                        (((x.TripTypeId == 1158|| (x.TripTypeId == 1160 && x.VehicleId != null)) && kmsource == 1) || (x.TripTypeId == 1159 && kmsource == 2)))
                group x by new {x.VehicleId, x.TripStartDate.Year}
                into g1
                select new
                {
                    VehicleId = g1.Key.VehicleId.Value,
                    TripYear = g1.Key.Year,
                    TotalKm = g1.Sum(x => x.TotalKmRun)
                };

            var data = from d in (from x in query

                group x by new {x.CategoryName, x.ClassName, x.VehicleId, x.VehicleNo, x.VehicleType, x.TripYear}
                into g
                select g)
                join l in km on new {d.Key.VehicleId, d.Key.TripYear} equals new {l.VehicleId, l.TripYear} into g1
                select new vwVehicleMonthRepairSummary
                {
                    VehicleId = d.Key.VehicleId,
                    VehicleType = d.Key.VehicleType,
                    CategoryName = d.Key.CategoryName,
                    ClassName = d.Key.ClassName,
                    VehicleNo = d.Key.VehicleNo,
                    TripYear = d.Key.TripYear,
                    Jan = d.Where(x => x.TripMonth == 1).Sum(x => x.Amount),
                    Feb = d.Where(x => x.TripMonth == 2).Sum(x => x.Amount),
                    Mar = d.Where(x => x.TripMonth == 3).Sum(x => x.Amount),
                    Apr = d.Where(x => x.TripMonth == 4).Sum(x => x.Amount),
                    May = d.Where(x => x.TripMonth == 5).Sum(x => x.Amount),
                    Jun = d.Where(x => x.TripMonth == 6).Sum(x => x.Amount),
                    Jul = d.Where(x => x.TripMonth == 7).Sum(x => x.Amount),
                    Aug = d.Where(x => x.TripMonth == 8).Sum(x => x.Amount),
                    Sep = d.Where(x => x.TripMonth == 9).Sum(x => x.Amount),
                    Oct = d.Where(x => x.TripMonth == 10).Sum(x => x.Amount),
                    Nov = d.Where(x => x.TripMonth == 11).Sum(x => x.Amount),
                    Dec = d.Where(x => x.TripMonth == 12).Sum(x => x.Amount),
                    TotalAmount = d.Sum(x => x.Amount),

                    TotalKm = g1.Sum(x => x.TotalKm)

                };
            return data;
        }

        public static IQueryable<vwVehicleJobtypeSummary> GetAllVehicleJobtypeSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId, long kmsource)
        {
            //Triplog: kmsource=1, Jobsheet:kmsource=2
            var vehrepo = repository.Queryable();
            var spareepo = repository.GetRepository<SpareLog>().Queryable();
            var tripsrepo = repository.GetRepository<VehicleMovementLog>().Queryable();

            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var query =
                from v in vehrepo
                join t in
                    spareepo.Where(x => x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) on v.Id equals t.VehicleId
                join o in objectids on v.Id equals o.ObjectId
                select new
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    VehicleType = v.fk_VehicleType == null ? null : v.fk_VehicleType.Name,
                    TripYear = t.VoucherDate.Year,
                    SpareNatureId = t.fk_Spare == null ? null : t.fk_Spare.SpareNatureId,
                    JobTypeId = t.fk_JobCard == null ? null : t.fk_JobCard.JobTypeId,

                    Amount = t.NetAmount
                };
            var km = from x in
                tripsrepo.Where(
                    x =>
                        (x.TripStartDate >= FromDate && x.TripStartDate <= ToDate) && !x.IsHired &&
                        (((x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null)) && kmsource == 1) || (x.TripTypeId == 1159 && kmsource == 2)))
                group x by new {x.VehicleId, x.TripStartDate.Year}
                into g1
                select new
                {
                    VehicleId = g1.Key.VehicleId.Value,
                    TripYear = g1.Key.Year,
                    TotalKm = g1.Sum(x => x.TotalKmRun)
                };

            var data = from d in (from x in query

                group x by new {x.CategoryName, x.ClassName, x.VehicleId, x.VehicleNo, x.VehicleType, x.TripYear}
                into g
                select g)
                join l in km on new {d.Key.VehicleId, d.Key.TripYear} equals new {l.VehicleId, l.TripYear} into g1
                select new vwVehicleJobtypeSummary
                {
                    VehicleId = d.Key.VehicleId,
                    VehicleType = d.Key.VehicleType,
                    CategoryName = d.Key.CategoryName,
                    ClassName = d.Key.ClassName,
                    VehicleNo = d.Key.VehicleNo,
                    TripYear = d.Key.TripYear,

                    //spare:1084/Labour:1083
                    TotSpareAmount = d.Where(x => x.SpareNatureId == 1084).Sum(x => x.Amount),
                    TotLabourAmount = d.Where(x => x.SpareNatureId == 1083).Sum(x => x.Amount),

                    //1318:General,1319:Accidental,1320:Capital,1339:Claim
                    TotGeneralAmount = d.Where(x => x.JobTypeId == 1318).Sum(x => x.Amount),
                    TotAccidentAmount = d.Where(x => x.JobTypeId == 1319).Sum(x => x.Amount),
                    TotCapitalAmount = d.Where(x => x.JobTypeId == 1320).Sum(x => x.Amount),
                    TotClaimAmount = d.Where(x => x.JobTypeId == 1339).Sum(x => x.Amount),
                    TotOtherAmount = d.Where(x => x.JobTypeId > 1339).Sum(x => x.Amount),

                    TotAmount = d.Sum(x => x.Amount),
                    TotKm = g1.Sum(x => x.TotalKm)

                };
            return data;
        }


        public static IQueryable<vwVehicleTriplogPerformanceSummary> GetAllVehicleTriplogPerformanceSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var vehdata = repository.Queryable();
            var tripsrepo = repository.GetRepository<VehicleMovementLog>().Queryable();
            var tyrerepo = repository.GetRepository<TyreLog>().Queryable();

            var repairrepo = repository.GetRepository<SpareLog>().Queryable();
            var duesrepo = repository.GetRepository<DueTransactionLog>().Queryable();
            var advrepo = repository.GetRepository<TripAdvanceLog>().Queryable();

            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);



            var data = from v in vehdata
                join o in objectids on v.Id equals o.ObjectId
                join tr in
                    tripsrepo.Where(
                        x =>
                            x.TripStartDate >= FromDate && x.TripStartDate <= ToDate &&
                            (x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null))) on
                    v.Id equals tr.VehicleId into g1
                join ty in
                    tyrerepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate && x.TyreStatusId == 1103 &&
                            x.VoucherTypeId != 42) on
                    v.Id equals ty.VehicleId into g2
                join rp in
                    repairrepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate &&
                            (x.VoucherTypeId == 22 || x.VoucherTypeId == 24)) on
                    v.Id equals rp.VehicleId into g3

                join jq in
                    duesrepo.Where(x => x.PaidDate >= FromDate && x.PaidDate <= ToDate) on
                    v.Id equals jq.VehicleId into g4

                join jq in
                    advrepo.Where(
                        x =>
                            x.AdvanceDate >= FromDate && x.AdvanceDate <= ToDate &&
                            (x.AdvanceTypeId == 11 || x.AdvanceTypeId == 13
                             || x.AdvanceTypeId == 59 || x.AdvanceTypeId == 14 || x.AdvanceTypeId == 15 ||
                             x.AdvanceTypeId == 16 || x.AdvanceTypeId == 17)) on
                    v.Id equals jq.VehicleId into g5

                select new vwVehicleTriplogPerformanceSummary
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    TLCount = g1.Count(),
                    Days = g1.Sum(x => DbFunctions.DiffDays(x.TripStartDate, x.UnloadingDate) + 1),
                    TotKmRun = g1.Sum(x => x.TotalKmRun),
                    TotAdv = g1.Sum(x => x.TripAdvances.Sum(y => y.CashAmount)),
                    TotExp = g1.Sum(x => x.TripExpenses.Sum(y => y.SettledAmount)),

                    TotFreight = g1.Sum(x => x.CNFreight),


                    cNet = g1.Sum(x => x.CNFreight) - g1.Sum(x => x.TripExpenses.Sum(y => y.SettledAmount)),
                    //shall minus exp

                    TotTyreExp = g2.Sum(x => x.NetAmount),

                    TotRepairExp = g3.Sum(x => x.NetAmount),
                    TotDuesExp = g4.Sum(x => x.DueAmount),
                    TotDriverExp =
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                    TotGenExp = 0,

                    TotEMIExp = 0,
                    FNet =
                        g1.Sum(x => x.CNFreight) - g1.Sum(x => x.TripExpenses.Sum(y => y.SettledAmount)) -
                        g2.Sum(x => x.NetAmount) - g3.Sum(x => x.NetAmount) - g4.Sum(x => x.DueAmount) -
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount))
                };
            return data;
        }

        public static IQueryable<vwVehicleTripExpBreakupSummary> GetAllVehicleTripExpBreakupSummary(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var vehdata = repository.Queryable();
            var tripsrepo = repository.GetRepository<VehicleTripSettlement>().Queryable();
            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data =
                from o in ccm
                join vv in vehdata on o.ObjectId equals vv.Id
                join v in
                    tripsrepo.Where(x => (x.StartDate >= FromDate && x.StartDate <= ToDate) && x.SettleDate != null) on
                    o.ObjectId equals v.VehicleId
                group v by new {o.fk_Category.CategoryName, o.fk_Class.ClassName, v.VehicleId, v.fk_Vehicle.VehicleNo}
                into g
                select new vwVehicleTripExpBreakupSummary
                {
                    CategoryName = g.Key.CategoryName,
                    ClassName = g.Key.ClassName,
                    VehicleId = g.Key.VehicleId,
                    VehicleNo = g.Key.VehicleNo,
                    TLCount = g.Select(x => x.TripLogs).Count(),
                    Days = g.Sum(x => DbFunctions.DiffDays(x.StartDate, x.EndDate)),
                    TotKmRun = g.Sum(x => x.TotalKmRun),
                    FixExp =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "FixExp")
                                    .Sum(x => x.SettledAmount)),
                    TollTax =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "TollTax")
                                    .Sum(x => x.SettledAmount)),
                    Diesel =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Diesel")
                                    .Sum(x => x.SettledAmount)),
                    Salary =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Salary")
                                    .Sum(x => x.SettledAmount)),
                    Fooding =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Fooding")
                                    .Sum(x => x.SettledAmount)),
                    Welfare =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Welfare")
                                    .Sum(x => x.SettledAmount)),
                    Entry =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Entry")
                                    .Sum(x => x.SettledAmount)),
                    Phone =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Phone")
                                    .Sum(x => x.SettledAmount)),
                    Challan =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Challan")
                                    .Sum(x => x.SettledAmount)),
                    OverLd =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "OverLd")
                                    .Sum(x => x.SettledAmount)),
                    Repair =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Repair")
                                    .Sum(x => x.SettledAmount)),
                    Others =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Others")
                                    .Sum(x => x.SettledAmount)),
                    Total =
                        g.Sum(
                            y =>
                                y.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Total")
                                    .Sum(x => x.SettledAmount))

                };
            return data;
        }

        public static IQueryable<vwVehicleTyreModelCount> GetAllVehicleTyreModelCount(
            this IRepository<VehicleMaster> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var vehdata = repository.Queryable();
            var vehmodel = repository.GetRepository<VehicleModel>().Queryable();
            var tyrerepo = repository.GetRepository<TyreLog>().Queryable();
            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data =
                from o in ccm
                join v in vehdata on o.ObjectId equals v.Id
                join vm in vehmodel on v.VehicleModelId equals vm.Id
                join t in (from x in tyrerepo.Where(x => x.NextLogId == null)
                    group x by x.VehicleId
                    into g
                    select new
                    {
                        VehicleId = g.Key,
                        NoofTyres = g.Where(x => !x.IsStepney).Count(),
                        Noofstpny = g.Where(x => x.IsStepney).Count()

                    }) on v.Id equals t.VehicleId
                select new vwVehicleTyreModelCount
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.VehicleNo,
                    VehicleModel = vm.ModelName,
                    BNoOfTyres = (int) vm.NoOfTyres,
                    BNoOfStpny = (int) vm.NoOfStphny,
                    ANoOfTyres = t.NoofTyres,
                    ANoOfStpny = t.Noofstpny,
                    StpnyDiff = (int) vm.NoOfTyres - t.NoofTyres,
                    TyreDiff = (int) vm.NoOfStphny - t.Noofstpny
                };
            return data;
        }


        public static IQueryable<vwTyreLifePerformanceBrandwiseAnalysis> GetAllTyreLifePerformanceBrandwiseAnalysis(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, bool IsScrapDate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();


            var data =
                from tm in
                    tmrepo.Where(
                        x =>
                            (x.fk_PurchaseTyreLog.VoucherDate >= FromDate &&
                             x.fk_PurchaseTyreLog.VoucherDate <= ToDate & !IsScrapDate) ||
                            (x.S_VoucherTypeId == 37 && x.S_VoucherDate >= FromDate &&
                             x.S_VoucherDate <= ToDate & IsScrapDate))
                join t in tyrerepo
                    on tm.Id equals t.TyreId
                group t by
                    new
                    {
                        t.TyreId,
                        t.TyreNo,
                        t.TyreLife,
                        tm.fk_Brand.BrandName,
                        Manufacturer = tm.fk_Brand.fk_Manufacturer.Name,
                        t.fk_CreditAccount.FleetAcName
                    }
                into g
                select new vwTyreLifePerformanceBrandwiseAnalysis
                {
                    TyreId = g.Key.TyreId,
                    TyreNo = g.Key.TyreNo,
                    TyreLife = g.Key.TyreLife,
                    BrandName = g.Key.BrandName,
                    Manufacturer = g.Key.Manufacturer,
                    SupplierName = g.Key.FleetAcName,
                    TyreCost = g.Sum(x => x.NetAmount),
                    TyreTPCost = g.Sum(x => x.TransferPrice),
                    TyreScrapCost = g.Sum(x => x.ScrapCost),
                    TyreNetCost = g.Sum(x => x.NetAmount) - g.Sum(x => x.TransferPrice) - g.Sum(x => x.ScrapCost),
                    TyreKmRun = g.Sum(x => x.KmRun),
                    TyreUsedMonth = g.Sum(x => x.ReceiptMonth),
                    TyreScrapDate = g.Select(x => x.fk_Tyre.fk_PurchaseTyreLog.VoucherDate).FirstOrDefault()
                };
            return data;
        }

        public static IQueryable<vwTyrePerformanceVehiclewise> GetAllTyrePerformanceVehiclewise(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId, bool isissuedate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();
            var tpl = repository.GetRepository<TyreLifePerformanceLog>().Queryable();
            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data = from o in ccm
                join t in tyrerepo.Where(x => (x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) && isissuedate) on
                    o.ObjectId equals t.VehicleId
                join tm in tmrepo on t.TyreId equals tm.Id
                join tr in tyrerepo.Where(x => (x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) && !isissuedate)
                    on t.Id equals tr.PreviousLogId into r
                join l in (from ll in tpl
                    select new
                    {
                        ll.TyreId,
                        ll.Life,
                        PreviousMileage = ll.TyrePreviousMileage,
                        LifeMileage = ll.TyreLifeMileage,
                        ll.PurchaseAmount,
                        AvgCPKM =
                            ((ll.TyrePreviousMileage + ll.TyreLifeMileage) == 0
                                ? 0
                                : (ll.PurchaseAmount/(ll.TyrePreviousMileage + ll.TyreLifeMileage)))

                    })
                    on new {t.TyreId, t.TyreLife} equals new {l.TyreId, TyreLife = l.Life}
                    into g
                select new vwTyrePerformanceVehiclewise
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = t.VehicleId,
                    VehicleNo = t.fk_Vehicle.VehicleNo,
                    TyreId = t.TyreId,
                    TyreNo = t.TyreNo,
                    TyreLife = t.TyreLife,
                    Tyrestpny = t.IsStepney,
                    BrandName = tm.fk_Brand.BrandName,
                    TotalTyreCost = g.Select(x => x.PurchaseAmount).FirstOrDefault(),
                    TotalMileage = g.Select(x => x.PreviousMileage + x.LifeMileage).FirstOrDefault(),
                    LifeCPKM = g.Select(x => x.AvgCPKM).FirstOrDefault(),
                    KmRun = r.Select(x => x.KmRun).FirstOrDefault(),
                    CPKM = r.Select(x => x.KmRun).FirstOrDefault()*g.Select(x => x.AvgCPKM).FirstOrDefault()

                };
            return data;
        }

        public static IQueryable<vwTyreVehicleMileageSummary> GetAllTyreVehicleMileageSummary(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId, bool isissuedate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();
            var tpl = repository.GetRepository<TyreLifePerformanceLog>().Queryable();
            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var tdata = from o in ccm
                join t in tyrerepo.Where(x => (x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) && isissuedate) on
                    o.ObjectId equals t.VehicleId
                join tr in tyrerepo.Where(x => (x.VoucherDate >= FromDate && x.VoucherDate <= ToDate) && !isissuedate)
                    on t.Id equals tr.PreviousLogId into r
                join l in (from ll in tpl
                    select new
                    {
                        ll.TyreId,
                        ll.Life,
                        ll,
                        PreviousMileage = ll.TyrePreviousMileage,
                        LifeMileage = ll.TyreLifeMileage,
                        ll.PurchaseAmount,
                        AvgCPKM =
                            ((ll.TyrePreviousMileage + ll.TyreLifeMileage) == 0
                                ? 0
                                : (ll.PurchaseAmount/(ll.TyrePreviousMileage + ll.TyreLifeMileage))),
                        ll.LifeIssueCounts

                    })
                    on new {t.TyreId, t.TyreLife} equals new {l.TyreId, TyreLife = l.Life} into g

                select new
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleNo = t.fk_Vehicle.VehicleNo,
                    IsStepney = t.IsStepney,
                    TotalTyreCost = g.Select(x => x.PurchaseAmount).FirstOrDefault(),
                    TotalMileage = g.Select(x => x.PreviousMileage + x.LifeMileage).FirstOrDefault(),
                    LifeCPKM = g.Select(x => x.AvgCPKM).FirstOrDefault(),
                    KmRun = r.Select(x => x.KmRun).FirstOrDefault(),
                    CalcTyreCost = r.Select(x => x.KmRun).FirstOrDefault()*g.Select(x => x.AvgCPKM).FirstOrDefault(),
                    LifeIssueCounts = g.Select(x => x.LifeIssueCounts).FirstOrDefault()
                };
            var data =
                tdata.GroupBy(vv => new {vv.VehicleNo, vv.CategoryName, vv.ClassName})
                    .Select(g => new vwTyreVehicleMileageSummary
                    {
                        CategoryName = g.Key.CategoryName,
                        ClassName = g.Key.ClassName,
                        VehicleNo = g.Key.VehicleNo,
                        SingleIssueCount = g.Count(x => x.LifeIssueCounts == 1 && !x.IsStepney),
                        MultipleIssueCount = g.Count(x => x.LifeIssueCounts > 1 && !x.IsStepney),
                        STPNYIssueCount = g.Count(x => x.IsStepney),
                        SingleIssueMileage =
                            (int?) g.Where(x => x.LifeIssueCounts == 1 && !x.IsStepney).Sum(x => x.KmRun),
                        MultipleIssueMileage =
                            (int?) g.Where(x => x.LifeIssueCounts > 1 && !x.IsStepney).Sum(x => x.KmRun),
                        CalcTyreCost = g.Sum(x => x.CalcTyreCost)
                    });
            return data;
        }


        public static IQueryable<vwVehicleTripPerformanceDetail> GetAllVehicleTripPerformanceDetail(
            this IRepository<VehicleTripSettlement> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var tripsrepo = repository.Queryable();
            var tyrerepo = repository.GetRepository<TyreLog>().Queryable();
            var repairrepo = repository.GetRepository<SpareLog>().Queryable();
            var duesrepo = repository.GetRepository<DueTransactionLog>().Queryable();
            var driverrepo = repository.GetRepository<TripAdvanceLog>().Queryable();

            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data =
                from o in ccm
                join v in
                    tripsrepo.Where(x => (x.StartDate >= FromDate && x.StartDate <= ToDate) && x.SettleDate != null) on
                    o.ObjectId equals v.VehicleId
                join ty in
                    tyrerepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate && x.TyreStatusId == 1103 &&
                            x.VoucherTypeId != 42) on
                    v.Id equals ty.VehicleId into g2
                join rp in
                    repairrepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate &&
                            (x.VoucherTypeId == 22 || x.VoucherTypeId == 24)) on
                    v.Id equals rp.VehicleId into g3

                join jq in
                    duesrepo.Where(x => x.PaidDate >= FromDate && x.PaidDate <= ToDate) on
                    v.Id equals jq.VehicleId into g4

                join jq in
                    driverrepo.Where(
                        x =>
                            x.AdvanceDate >= FromDate && x.AdvanceDate <= ToDate &&
                            (x.AdvanceTypeId == 11 || x.AdvanceTypeId == 13
                             || x.AdvanceTypeId == 59 || x.AdvanceTypeId == 14 || x.AdvanceTypeId == 15 ||
                             x.AdvanceTypeId == 16 || x.AdvanceTypeId == 17)) on
                    v.Id equals jq.VehicleId into g5


                select new vwVehicleTripPerformanceDetail
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.fk_Vehicle.VehicleNo,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    SettledDate = v.SettleDate,
                    TLCount = v.TripLogs.Select(z => z.Id).Count(),
                    Days = DbFunctions.DiffDays(v.StartDate, v.EndDate) + 1,
                    TotKmRun = v.TotalKmRun,
                    TotFreight = v.BookingFreight,
                    TotExp = v.TripExpenseAmt,
                    cNet = v.BookingFreight - v.TripExpenseAmt,
                    TotTyreExp = g2.Sum(x => x.NetAmount),

                    TotRepairExp = g3.Sum(x => x.NetAmount),
                    TotDuesExp = g4.Sum(x => x.DueAmount),
                    TotDriverExp =
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                    TotGenExp = 0,

                    TotEMIExp = 0,
                    FNet =
                        v.BookingFreight - v.TripExpenseAmt - g2.Sum(x => x.NetAmount) - g3.Sum(x => x.NetAmount) -
                        g4.Sum(x => x.DueAmount) -
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                };
            return data;
        }

        public static IQueryable<vwDriverTripPerformanceDetail> GetAllDriverTripPerformanceDetail(
            this IRepository<VehicleTripSettlement> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var tripsrepo = repository.Queryable();
            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data =
                from o in ccm
                join v in
                    tripsrepo.Where(x => (x.StartDate >= FromDate && x.StartDate <= ToDate) && x.SettleDate != null) on
                    o.ObjectId equals v.Driver1Id
                select new vwDriverTripPerformanceDetail
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    DriverId = o.ObjectId,
                    DriverName = v.fk_DriverI == null ? null : v.fk_DriverI.DriverName,
                    VehicleId = v.VehicleId,
                    VehicleNo = v.fk_Vehicle == null ? null : v.fk_Vehicle.VehicleNo,

                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    SettledDate = v.SettleDate,
                    TLCount = v.TripLogs.Select(z => z.Id).Count(),
                    Days = DbFunctions.DiffDays(v.StartDate, v.EndDate) + 1,
                    TotKmRun = v.TotalKmRun,
                    Freight = v.BookingFreight,
                    TripAdv = v.TripAdvanceAmt,
                    TripExp = v.TripExpenseAmt,

                    Diff = v.TripAdvanceAmt - v.TripExpenseAmt,
                    FuelExp = v.FuelExpenseAmt,

                    BdgtFuelQty = v.TotalBudgetedFuelQty,
                    ActualFuelQty = v.ActualQty,
                    ExtraFuelQty = v.ExtraFuelQty

                };
            return data;
        }

        public static IQueryable<vwVehicleTripExpBreakupDetail> GetAllVehicleTripExpBreakupDetail(
            this IRepository<VehicleTripSettlement> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var tripsrepo = repository.Queryable();

            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data =
                from o in ccm
                join v in
                    tripsrepo.Where(x => (x.StartDate >= FromDate && x.StartDate <= ToDate) && x.SettleDate != null) on
                    o.ObjectId equals v.VehicleId
                select new vwVehicleTripExpBreakupDetail
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.fk_Vehicle.VehicleNo,
                    FromDate = v.StartDate,
                    ToDate = v.EndDate,
                    TLCount = v.TripLogs.Select(z => z.Id).Count(),
                    Days = DbFunctions.DiffDays(v.StartDate, v.EndDate) + 1,
                    TotKmRun = v.TotalKmRun,
                    TripNo = v.TripSheetNo,
                    RouteName = v.TripRoute,
                    DriverName = v.fk_DriverI.DriverName,
                    FixExp =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "FixExp")
                            .Sum(x => x.SettledAmount),
                    TollTax =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "TollTax")
                            .Sum(x => x.SettledAmount),
                    Diesel =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Diesel")
                            .Sum(x => x.SettledAmount),
                    Salary =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Salary")
                            .Sum(x => x.SettledAmount),
                    Fooding =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Fooding")
                            .Sum(x => x.SettledAmount),
                    Welfare =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Welfare")
                            .Sum(x => x.SettledAmount),
                    Entry =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Entry")
                            .Sum(x => x.SettledAmount),
                    Phone =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Phone")
                            .Sum(x => x.SettledAmount),
                    Challan =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Challan")
                            .Sum(x => x.SettledAmount),
                    OverLd =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "OverLd")
                            .Sum(x => x.SettledAmount),
                    Repair =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Repair")
                            .Sum(x => x.SettledAmount),
                    Others =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Others")
                            .Sum(x => x.SettledAmount),
                    Total =
                        v.TripExpenses.Where(x => x.fk_ExpenseType.fk_ExpenseCategory.Name == "Total")
                            .Sum(x => x.SettledAmount),

                };
            return data;
        }

        public static IQueryable<vwVehicleTriplogPerformanceDetail> GetAllVehicleTriplogPerformanceDetail(
            this IRepository<VehicleMovementLog> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var tripsrepo = repository.Queryable();
            var tyrerepo = repository.GetRepository<TyreLog>().Queryable();
            var repairrepo = repository.GetRepository<SpareLog>().Queryable();
            var duesrepo = repository.GetRepository<DueTransactionLog>().Queryable();
            var driverrepo = repository.GetRepository<TripAdvanceLog>().Queryable();

            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var data =
                from o in ccm
                join v in
                    tripsrepo.Where(
                        x =>
                            (x.TripStartDate >= FromDate && x.TripStartDate <= ToDate &&
                             (x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null)))) on o.ObjectId equals v.VehicleId
                join ty in
                    tyrerepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate && x.TyreStatusId == 1103 &&
                            x.VoucherTypeId != 42) on
                    v.Id equals ty.VehicleId into g2
                join rp in
                    repairrepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate &&
                            (x.VoucherTypeId == 22 || x.VoucherTypeId == 24)) on
                    v.Id equals rp.VehicleId into g3

                join jq in
                    duesrepo.Where(x => x.PaidDate >= FromDate && x.PaidDate <= ToDate) on
                    v.Id equals jq.VehicleId into g4

                join jq in
                    driverrepo.Where(
                        x =>
                            x.AdvanceDate >= FromDate && x.AdvanceDate <= ToDate &&
                            (x.AdvanceTypeId == 11 || x.AdvanceTypeId == 13
                             || x.AdvanceTypeId == 59 || x.AdvanceTypeId == 14 || x.AdvanceTypeId == 15 ||
                             x.AdvanceTypeId == 16 || x.AdvanceTypeId == 17)) on
                    v.Id equals jq.VehicleId into g5


                select new vwVehicleTriplogPerformanceDetail
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = v.Id,
                    VehicleNo = v.fk_Vehicle.VehicleNo,
                    StartDate = v.TripStartDate,
                    EndDate = v.UnloadingDate,
                    Days = DbFunctions.DiffDays(v.TripStartDate, v.UnloadingDate) + 1,
                    TotKmRun = v.TotalKmRun,
                    TotAdv = v.TripAdvances.Sum(x => x.CashAmount),
                    TotExp = v.TripExpenses.Sum(x => x.SettledAmount),

                    TotFreight = v.CNFreight,

                    cNet = v.CNFreight - v.TripExpenses.Sum(x => x.SettledAmount), //minus exp
                    TotTyreExp = g2.Sum(x => x.NetAmount),

                    TotRepairExp = g3.Sum(x => x.NetAmount),
                    TotDuesExp = g4.Sum(x => x.DueAmount),
                    TotDriverExp =
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                    TotGenExp = 0,

                    TotEMIExp = 0,
                    FNet =
                        v.CNFreight - v.TripExpenses.Sum(x => x.SettledAmount) - g2.Sum(x => x.NetAmount) -
                        g3.Sum(x => x.NetAmount) - g4.Sum(x => x.DueAmount) -
                        g5.Sum(x => ((x.AdvanceTypeId == 59 || x.AdvanceTypeId == 17) ? -x.CashAmount : x.CashAmount)),

                };
            return data;
        }



        public static IQueryable<vwTyreSAWithMovementDetails> GetAllTyreSAWithMovementDetails(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, bool IsScrapDate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();


            var tdata =
                from tm in
                    tmrepo.Where(
                        x =>
                            (x.fk_PurchaseTyreLog.VoucherDate >= FromDate &&
                             x.fk_PurchaseTyreLog.VoucherDate <= ToDate & !IsScrapDate) ||
                            (x.S_VoucherTypeId == 37 && x.S_VoucherDate >= FromDate &&
                             x.S_VoucherDate <= ToDate & IsScrapDate))
                join t in tyrerepo on tm.Id equals t.TyreId
                // group t by new { t.TyreId, t.TyreNo, t.TyreLife, tm.fk_Brand.BrandName,tm.S_VoucherDate}
                //into g
                select new
                {
                    TyreNo = t.TyreNo,
                    TyreLife = t.TyreLife,
                    BrandName = tm.fk_Brand.BrandName,
                    TyreCost = t.VoucherTypeId == 37 ? 0 : t.NetAmount,
                    ScrapDate = t.VoucherTypeId == 37 ? t.VoucherDate : (DateTime?) null,
                    ScrapAmount = t.VoucherTypeId == 37 ? t.NetAmount : 0,
                    MonthUsed = t.ReceiptMonth,
                    Mileage = t.KmRun,
                    NetCost = t.VoucherTypeId == 37 ? -t.NetAmount : t.NetAmount,
                };
            var data = from t in tdata
                group t by new {t.TyreNo, t.BrandName}
                into g
                select new vwTyreSAWithMovementDetails
                {
                    TyreNo = g.Key.TyreNo,
                    BrandName = g.Key.BrandName,
                    MaxLife = g.Max(x => x.TyreLife),
                    TyreCost = g.Sum(x => x.TyreCost),
                    ScrapDate = g.Select(x => x.ScrapDate).FirstOrDefault(),
                    ScrapAmount = g.Select(x => x.ScrapAmount).FirstOrDefault(),
                    TotalMonthUsed = (int?) g.Sum(x => x.MonthUsed),
                    TotalMileage = g.Sum(x => x.Mileage),
                    NetCost = g.Sum(x => x.NetCost),
                };

            return data;
        }


        public static IQueryable<vwTyreSAWithMovementSummary> GetAllTyreSAWithMovementSummary(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, bool IsScrapDate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();


            var tdata =
                from tm in
                    tmrepo.Where(
                        x =>
                            (x.fk_PurchaseTyreLog.VoucherDate >= FromDate &&
                             x.fk_PurchaseTyreLog.VoucherDate <= ToDate & !IsScrapDate) ||
                            (x.S_VoucherTypeId == 37 && x.S_VoucherDate >= FromDate &&
                             x.S_VoucherDate <= ToDate & IsScrapDate))
                join t in tyrerepo on tm.Id equals t.TyreId
                select new
                {
                    TyrePattern = tm.fk_Brand.fk_BrandNature == null ? null : tm.fk_Brand.fk_BrandNature.Name,
                    TyreId = t.TyreId,
                    TyreCost = t.VoucherTypeId == 37 ? 0 : t.NetAmount,
                    ScrapAmount = t.VoucherTypeId == 37 ? t.NetAmount : 0,
                    MonthUsed = t.ReceiptMonth,
                    Mileage = t.KmRun,
                    NetCost = t.VoucherTypeId == 37 ? -t.NetAmount : t.NetAmount,
                };
            var ldata = from t in tdata
                group t by new {t.TyreId, t.TyrePattern}
                into g
                select new
                {
                    TyreId = g.Key.TyreId,
                    TyrePattern = g.Key.TyrePattern,
                    TyreCost = g.Sum(x => x.TyreCost),
                    ScrapAmount = g.Select(x => x.ScrapAmount).FirstOrDefault(),
                    MonthUsed = (int?) g.Sum(x => x.MonthUsed),
                    Mileage = g.Sum(x => x.Mileage),
                };
            var data = from t in ldata
                group t by new {t.TyrePattern}
                into g
                select new vwTyreSAWithMovementSummary
                {
                    TyrePattern = g.Key.TyrePattern,
                    Qty = g.Count(),
                    TyreCost = g.Sum(x => x.TyreCost),
                    ScrapAmount = g.Sum(x => x.ScrapAmount),
                    TotalMonthUsed = (int?) g.Sum(x => x.MonthUsed),
                    TotalMileage = g.Sum(x => x.Mileage),
                    AvgCost = (!g.Any() ? 0 : g.Sum(x => x.TyreCost)/g.Count()),
                    AvgScrapCost = (!g.Any() ? 0 : g.Sum(x => x.ScrapAmount)/g.Count()),
                    AvgMileage = (!g.Any() ? 0 : g.Sum(x => x.Mileage)/g.Count()),
                    AvgNetCost = (!g.Any() ? 0 : (g.Sum(x => x.TyreCost) + g.Sum(x => x.ScrapAmount))/g.Count())
                };

            return data;
        }



        public static IQueryable<vwTyreSAwithLifeSpanDetail> GetAllTyreSAwithLifeSpanDetail(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, bool IsScrapDate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();


            var tdata =
                from tm in
                    tmrepo.Where(
                        x =>
                            (x.fk_PurchaseTyreLog.VoucherDate >= FromDate &&
                             x.fk_PurchaseTyreLog.VoucherDate <= ToDate & !IsScrapDate) ||
                            (x.S_VoucherTypeId == 37 && x.S_VoucherDate >= FromDate &&
                             x.S_VoucherDate <= ToDate & IsScrapDate))
                join t in tyrerepo on tm.Id equals t.TyreId
                select new
                {
                    TyreNo = t.TyreNo,
                    TyreLife = t.TyreLife,
                    BrandName = tm.fk_Brand.BrandName,
                    TyreCost = t.VoucherTypeId == 37 ? 0 : t.NetAmount,
                    ScrapDate = t.VoucherTypeId == 37 ? t.VoucherDate : (DateTime?) null,
                    ScrapAmount = t.VoucherTypeId == 37 ? t.NetAmount : 0,
                    MonthUsed = t.ReceiptMonth,
                    M0 = t.TyreLife == 0 ? t.KmRun : (long?) null,
                    M1 = t.TyreLife == 1 ? t.KmRun : (long?) null,
                    M2 = t.TyreLife == 2 ? t.KmRun : (long?) null,
                    M3 = t.TyreLife == 3 ? t.KmRun : (long?) null,
                    M4 = t.TyreLife == 4 ? t.KmRun : (long?) null,
                    M5 = t.TyreLife == 5 ? t.KmRun : (long?) null,
                    TotalMileage = t.KmRun,
                    NetCost = t.VoucherTypeId == 37 ? -t.NetAmount : t.NetAmount
                };
            var data = from t in tdata
                group t by new {t.TyreNo, t.BrandName}
                into g
                select new vwTyreSAwithLifeSpanDetail
                {
                    TyreNo = g.Key.TyreNo,
                    BrandName = g.Key.BrandName,
                    ScrapDate = g.Select(x => x.ScrapDate).FirstOrDefault(),
                    MaxLife = g.Max(x => x.TyreLife),
                    TotalMonthUsed = (int?) g.Sum(x => x.MonthUsed),
                    TyreCost = g.Sum(x => x.TyreCost),
                    ScrapAmount = g.Sum(x => x.ScrapAmount),
                    LM0 = g.Sum(x => x.M0),
                    LM1 = g.Sum(x => x.M1),
                    LM2 = g.Sum(x => x.M2),
                    LM3 = g.Sum(x => x.M3),
                    LM4 = g.Sum(x => x.M4),
                    LM5 = g.Sum(x => x.M5),
                    TotalMileage = g.Sum(x => x.TotalMileage),
                    NetCost = g.Sum(x => x.NetCost),
                };

            return data;
        }

        public static IQueryable<vwTyreSAwithLifeSpanSummary> GetAllTyreSAwithLifeSpanSummary(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, bool IsScrapDate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();


            var tdata =
                from tm in
                    tmrepo.Where(
                        x =>
                            (x.fk_PurchaseTyreLog.VoucherDate >= FromDate &&
                             x.fk_PurchaseTyreLog.VoucherDate <= ToDate & !IsScrapDate) ||
                            (x.S_VoucherTypeId == 37 && x.S_VoucherDate >= FromDate &&
                             x.S_VoucherDate <= ToDate & IsScrapDate))
                join t in tyrerepo on tm.Id equals t.TyreId
                select new
                {
                    TyreId = t.TyreId,
                    TyreLife = t.TyreLife,
                    TyreCost = t.VoucherTypeId == 37 ? 0 : t.NetAmount,
                    ScrapAmount = t.VoucherTypeId == 37 ? t.NetAmount : 0,
                    TotalMileage = t.KmRun,
                    NetCost = t.VoucherTypeId == 37 ? -t.NetAmount : t.NetAmount
                };

            var ldata = from t in tdata
                group t by new {t.TyreLife}
                into g
                select new
                {
                    TyreLife = g.Key.TyreLife,
                    TyreCount = g.Select(x => x.TyreId).Distinct().Count(),
                    TyreCost = g.Sum(x => x.TyreCost),
                    ScrapCost = g.Max(x => x.TyreLife),
                    NetCost = (int?) g.Sum(x => x.NetCost),
                    Mileage = g.Sum(x => x.TyreCost),

                };

            var data = from t in ldata
                select new vwTyreSAwithLifeSpanSummary
                {
                    TyreLife = t.TyreLife,
                    TyreCount = t.TyreCount,
                    AvgTyreCost = t.TyreCount == 0 ? 0 : t.TyreCost/t.TyreCount,
                    AvgScrapCost = t.TyreCount == 0 ? 0 : t.ScrapCost/t.TyreCount,
                    AvgNetCost = t.TyreCount == 0 ? 0 : t.NetCost/t.TyreCount,
                    AvgMileage = t.TyreCount == 0 ? 0 : t.Mileage/t.TyreCount
                };

            return data;
        }


        public static IQueryable<vwTyreSABrandwiseSummary> GetAllTyreSABrandwiseSummary(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, bool IsScrapDate)
        {
            var tyrerepo = repository.Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();


            var tdata =
                from tm in
                    tmrepo.Where(
                        x =>
                            (x.fk_PurchaseTyreLog.VoucherDate >= FromDate &&
                             x.fk_PurchaseTyreLog.VoucherDate <= ToDate & !IsScrapDate) ||
                            (x.S_VoucherTypeId == 37 && x.S_VoucherDate >= FromDate &&
                             x.S_VoucherDate <= ToDate & IsScrapDate))
                join t in tyrerepo on tm.Id equals t.TyreId
                select new
                {
                    TyreId = t.TyreId,
                    TyreLife = t.TyreLife,
                    BrandName = tm.fk_Brand.BrandName,
                    TyreCost = t.VoucherTypeId == 37 ? 0 : t.NetAmount,
                    ScrapDate = t.VoucherTypeId == 37 ? t.VoucherDate : (DateTime?) null,
                    ScrapAmount = t.VoucherTypeId == 37 ? t.NetAmount : 0,
                    MonthUsed = t.ReceiptMonth,
                    M0 = t.TyreLife == 0 ? t.KmRun : (long?) null,
                    M1 = t.TyreLife == 1 ? t.KmRun : (long?) null,
                    M2 = t.TyreLife == 2 ? t.KmRun : (long?) null,
                    M3 = t.TyreLife == 3 ? t.KmRun : (long?) null,
                    M4 = t.TyreLife == 4 ? t.KmRun : (long?) null,
                    M5 = t.TyreLife == 5 ? t.KmRun : (long?) null,
                    TotalMileage = t.KmRun,
                    NetCost = t.VoucherTypeId == 37 ? -t.NetAmount : t.NetAmount
                };
            var data = from t in tdata
                group t by new {t.BrandName}
                into g
                select new vwTyreSABrandwiseSummary
                {
                    BrandName = g.Key.BrandName,
                    TyreCount = g.Select(x => x.TyreId).Distinct().Count(),
                    TyreCost = g.Sum(x => x.TyreCost),
                    ScrapAmount = g.Sum(x => x.ScrapAmount),
                    LM0 = g.Sum(x => x.M0),
                    LM1 = g.Sum(x => x.M1),
                    LM2 = g.Sum(x => x.M2),
                    LM3 = g.Sum(x => x.M3),
                    LM4 = g.Sum(x => x.M4),
                    LM5 = g.Sum(x => x.M5),
                    TotalMileage = g.Sum(x => x.TotalMileage),
                    NetCost = g.Sum(x => x.NetCost),
                };

            return data;
        }

        public static IQueryable<vwTyreExpectedLife> GetAllTyreExpectedLife(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var tyrerepo = repository.Queryable();
            var tcrepo = repository.GetRepository<TyreCheck>().Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();
            var bmrepo = repository.GetRepository<BrandMaster>().Queryable();

            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var ldata =
                from t in tyrerepo.Where(x => x.NextLogId == null)
                join tm in tmrepo on t.TyreId equals tm.Id
                join bm in bmrepo on tm.BrandId equals bm.Id
                join tc in tcrepo.Where(x => x.NextLogId == null).Select(x => new
                {
                    WP = x.fk_WheelPosition.Name,
                    CurNSD = x.TreadDepth,
                    TyreId = x.TyreId
                }) on t.TyreId equals tc.TyreId into g
                join tt in (from ttt in tyrerepo
                    group ttt by ttt.TyreId
                    into g1
                    select new
                    {
                        TyreId = g1.Key,
                        TotMileage = g1.Sum(x => x.KmRun)
                    })
                    on t.TyreId equals tt.TyreId into g2
                select new
                {
                    TyreNo = t.TyreNo,
                    VehicleId = t.VehicleId,
                    VehicleNo = t.fk_Vehicle.VehicleNo,
                    BrandName = bm.BrandName,
                    WP = g.Select(x => x.WP).FirstOrDefault(),
                    CurNSD = g.Select(x => x.CurNSD).FirstOrDefault(),
                    MinNSD = bm.MinNSD,
                    NSDLeft = g.Select(x => x.CurNSD).FirstOrDefault() - bm.MinNSD,
                    BdgtdNSD = bm.StandardThreadDepth,
                    TyreErosion = bm.StandardThreadDepth - g.Select(x => x.CurNSD).FirstOrDefault(),
                    TotalMileage = g2.Select(x => x.TotMileage).FirstOrDefault(),
                };
            var data = from o in ccm
                join t in ldata on o.ObjectId equals t.VehicleId
                select new vwTyreExpectedLife
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = t.VehicleId,
                    VehicleNo = t.VehicleNo,
                    TyreNo = t.TyreNo,
                    BrandName = t.BrandName,
                    WP = t.WP,
                    CurNSD = t.CurNSD,
                    BdgtdNSD = t.BdgtdNSD,
                    TyreErosion = t.TyreErosion,
                    TotalMileage = t.TotalMileage,
                    EMLPerMM = (t.TyreErosion == 0 ? 0 : t.TotalMileage/t.TyreErosion),
                    ProjectedKM = (long?) (t.NSDLeft*(t.TyreErosion == 0 ? 0 : t.TotalMileage/t.TyreErosion))
                };

            return data;
        }


        public static IQueryable<vwRunningTyreTreadwearStatus> GetAllRunningTyreTreadwearStatus(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, string classIds, string accountIds, long categoryId)
        {
            var tyrerepo = repository.Queryable();
            var tcrepo = repository.GetRepository<TyreCheck>().Queryable();
            var tmrepo = repository.GetRepository<TyreMaster>().Queryable();
            var bmrepo = repository.GetRepository<BrandMaster>().Queryable();

            var ccm = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var ldata =
                from t in tyrerepo.Where(x => x.NextLogId == null)
                join tm in tmrepo on t.TyreId equals tm.Id
                join bm in bmrepo on tm.BrandId equals bm.Id
                join tc in tcrepo.Where(x => x.NextLogId == null).Select(x => new
                {
                    WP = x.fk_WheelPosition.Name,
                    WPDate = x.CheckDate,
                    NSD = x.TreadDepth,
                    KmReading = x.KmRun,
                    TyreId = x.TyreId
                }) on t.TyreId equals tc.TyreId into g

                select new
                {
                    TyreNo = t.TyreNo,
                    VehicleId = t.VehicleId,
                    VehicleNo = t.fk_Vehicle.VehicleNo,
                    BrandName = bm.BrandName,
                    OnDate = t.VoucherDate,
                    OnKm = t.KmReading,
                    R = t.IsRemoulded ? "Y" : "",
                    S = t.IsStepney ? "Y" : "",
                    TyreLife = t.TyreLife,
                    WP = g.Select(x => x.WP).FirstOrDefault(),
                    WPDate = g.Select(x => x.WPDate).FirstOrDefault(),
                    NSD = g.Select(x => x.NSD).FirstOrDefault(),
                    KmReading = g.Select(x => x.KmReading).FirstOrDefault()

                };
            var data = from o in ccm
                join t in ldata on o.ObjectId equals t.VehicleId
                select new vwRunningTyreTreadwearStatus
                {
                    CategoryName = o.fk_Category.CategoryName,
                    ClassName = o.fk_Class.ClassName,
                    VehicleId = t.VehicleId,
                    VehicleNo = t.VehicleNo,
                    TyreNo = t.TyreNo,
                    BrandName = t.BrandName,
                    OnDate = t.OnDate,
                    OnKm = t.OnKm,
                    WP = t.WP,
                    NSD = t.NSD,
                    KmReading = t.KmReading,
                    TyreLife = t.TyreLife,
                    R = t.R,
                    S = t.S
                };

            return data;
        }

        public static IQueryable<vwTyreStockLedgerNew> GetAllTyreStockLedgerNew(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, long accountId)
        {
            var tyrerepo = repository.Queryable();
            var arepo = repository.GetRepository<Ledger>().Queryable();
            var opdata =
                from o in
                    (from t in
                        tyrerepo.Where(
                            x =>
                                x.VoucherDate < FromDate &&
                                (x.DebitAccountId == accountId || x.CreditAccountId == accountId))
                        select new
                        {
                            DrId = accountId,
                            InQty = t.DebitAccountId == accountId ? 1 : 0,
                            OutQty = t.CreditAccountId == accountId ? 1 : 0,
                            DiffQty = t.DebitAccountId == accountId ? 1 : -1,
                            Amount = t.DebitAccountId == accountId ? t.NetAmount  : -t.NetAmount
                        })

                group o by o.DrId
                into opg
                select new vwTyreStockLedgerNew
                {
                    StoreName = arepo.Where(x => x.Id == accountId).Select(x => x.FleetAcName).FirstOrDefault(),
                    RefNo = "Opening As of ",
                    RefDate = FromDate,
                    Type = "",
                    Particulars = "",
                    InQty = opg.Sum(x => x.InQty),
                    OutQty = opg.Sum(x => x.OutQty),
                    DiffQty = opg.Sum(x => x.DiffQty),
                    StockValue = opg.Sum(x => x.Amount),
                    SortOrderId = 0
                };

            var ldata =
                from t in
                    tyrerepo.Where(
                        x =>
                            x.VoucherDate >= FromDate && x.VoucherDate <= ToDate &&
                            (x.DebitAccountId == accountId || x.CreditAccountId == accountId))
                select new
                {
                    RefNo = t.VoucherNo,
                    RefDate = t.VoucherDate,
                    Type = t.fk_VoucherType.VoucherTypeName,
                    ParticularId = t.VoucherTypeId == 34 ? t.VehicleId : t.CreditAccountId,
                    DrId = t.DebitAccountId,
                    //CrId = t.CreditAccountId,
                    InQty = t.DebitAccountId == accountId ? 1 : 0,
                    OutQty = t.CreditAccountId == accountId ? 1 : 0,
                    DiffQty= t.DebitAccountId == accountId ? 1 : -1,
                    Amount = t.DebitAccountId == accountId ? t.NetAmount : -t.NetAmount

                };
            var data = from t in ldata
                join dr in arepo on accountId equals dr.Id
                join p in arepo on t.ParticularId equals p.Id 
                group t by new { StoreName=dr.FleetAcName,t.RefNo,t.RefDate,t.Type, Particulars=p.FleetAcName }
                into g
                orderby g.Key.RefDate ascending
                select new vwTyreStockLedgerNew
                {
                    StoreName = g.Key.StoreName,
                    RefNo = g.Key.RefNo,
                    RefDate = g.Key.RefDate,
                    Type = g.Key.Type,
                    Particulars = g.Key.Particulars,
                    InQty = g.Sum(x=>x.InQty),
                    OutQty = g.Sum(x => x.OutQty),
                    DiffQty = g.Sum(x => x.DiffQty),
                    StockValue = g.Sum(x => x.Amount),
                    SortOrderId = 2
                };

            return opdata.Union(data);
        }

        public static IQueryable<vwTyreStockLedgerNewSummary> GetAllTyreStockLedgerNewSummary(
            this IRepository<TyreLog> repository,
            DateTime FromDate, DateTime ToDate, string accountIds)
        {
            var tyrerepo = repository.Queryable();
            var arepo = repository.GetRepository<Ledger>().Queryable();
            var opdata =
                from o in
                    (from a in arepo.Where(x => accountIds.Contains(x.Id.ToString()))
                        from t in tyrerepo.Where(x => x.VoucherDate < FromDate)
                        where a.Id == t.DebitAccountId || a.Id == t.CreditAccountId
                        select new
                        {
                            StoreName = a.FleetAcName,
                            OpQty = t.DebitAccountId == a.Id ? 1 : -1,
                            OpAmount = t.DebitAccountId == a.Id ? t.NetAmount : -t.NetAmount
                        })
                group o by o.StoreName
                into opg
                select new
                {
                    StoreName = opg.Key,
                    OpQty = opg.Sum(x => x.OpQty),
                    OpAmount = opg.Sum(x => x.OpAmount),
                };

            var tdata = from t in (
                from a in arepo.Where(x => accountIds.Contains(x.Id.ToString()))
                from t in tyrerepo.Where(x => x.VoucherDate >= FromDate && x.VoucherDate <= ToDate)
                where a.Id == t.DebitAccountId || a.Id == t.CreditAccountId
                select new
                {
                    StoreName = a.FleetAcName,
                    InQty = t.DebitAccountId == a.Id ? 1 : 0,
                    OutQty = t.CreditAccountId == a.Id ? 1 : 0,
                    Closing = t.DebitAccountId == a.Id ? 1 : -1,
                    Amount = t.DebitAccountId == a.Id ? t.NetAmount : -t.NetAmount


                })
                group t by t.StoreName
                into tt
                select new
                {
                    StoreName = tt.Key,
                    InQty = tt.Sum(x => x.InQty),
                    OutQty = tt.Sum(x => x.OutQty),
                    QtyDiff = tt.Sum(x => x.Closing),
                    Amount = tt.Sum(x => x.Amount),
                };

            var data = from o in opdata
                join t in tdata on o.StoreName equals t.StoreName
                orderby o.StoreName ascending
                select new vwTyreStockLedgerNewSummary
                {
                    StoreName = o.StoreName,
                    OpQty = o.OpQty,
                    InQty = t.InQty,
                    OutQty = t.OutQty,
                    Closing = t.QtyDiff + o.OpQty,
                    StockValue = t.Amount + o.OpAmount
                };

            return data;
        }

       
    }
}

