using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using AutoMapper.Mappers;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Reports.ViewModels.Finance;

namespace TrackoAPI.Repository
{
   public static class LedgerRepository
    {
        public static IQueryable<Ledger> GetAllLedgerByGroupId(this IRepository<Ledger> repository,long id) => repository.Queryable().Where(x => id == x.GroupId);
        public static IQueryable<Ledger> GetAllLedgerByGroupCode(this IRepository<Ledger> repository, string code) => repository.Queryable().Where(x => code == x.fk_Group.Alias);
        public static IQueryable<Ledger> GetLedgerByRoleId(this IRepository<Ledger> repository, long roleid) => repository.Queryable().Where(x => roleid == x.AccountRoleId);

        public static async Task MapLedgerToDefaultGroupClass(this IRepositoryAsync<Ledger> repository, long ledgerId,long? newGroupId,long? oldGroupId)
        {
            if (newGroupId > 0)
            {
                var clsRepo = repository.GetRepository<ObjectClassMap>();
                var ctgQuery = repository.GetRepository<ObjectCategory>().Queryable().Where(x =>
                    x.RoleId == newGroupId && x.RoleTypeId == 1146 && x.Objects.All(y => y.ObjectId != ledgerId));
                if (await ctgQuery.AnyAsync())
                {
                    var query = ctgQuery.SelectMany(x => x.ObjectClasses)
                        .Where(x => x.ClassName == "All").Select(x => new
                        {
                            ClassId = x.Id,
                            CategoryId = x.CategoryId,
                            x.ClassName
                        });
                    if (query.Any())
                    {
                        var cls = await query.ToListAsync();
                        var list = cls.Select(x => new ObjectClassMap
                        {
                            Id = 0,
                            ObjectState = ObjectState.Added,
                            ObjectId = ledgerId,
                            ClassId = x.ClassId,
                            CategoryId = x.CategoryId
                        }).ToList();
                        clsRepo.InsertRange(list);//Insert for new group
                        await repository.UOW.SaveChangesAsync();
                    }
                }

            }
            //if (newGroupId > 0)
            //{
            //    var clsRepo = repository.GetRepository<ObjectClassMap>();
            //    var cls = await repository.GetRepository<ObjectClass>()
            //        .Queryable()
            //        .Include(x => x.Category)
            //        .Where(x => x.ClassName == "All" && x.RoleId == newGroupId && x.Category.RoleTypeId == 1146 && x.ObjectMappings.All(y => y.ObjectId != ledgerId)).Select(x => new
            //        {
            //            ClassId = x.Id,
            //            CategoryId = x.CategoryId
            //        }).ToListAsync();
            //    if (cls.Any())
            //    {
            //        var list = cls.Select(x => new ObjectClassMap
            //        {
            //            Id = 0,
            //            ObjectState = ObjectState.Added,
            //            ObjectId = ledgerId,
            //            ClassId = x.ClassId,
            //            CategoryId = x.CategoryId
            //        }).ToList();
            //        clsRepo.InsertRange(list);//Insert for new group
            //        await repository.UOW.SaveChangesAsync();
            //    }
            //}
            
            if (newGroupId != oldGroupId&&oldGroupId>0)
            {
                await repository.ExecuteSqlAsync(
                    $"DELETE O FROM [dbo].[tObjectClassMap] AS O JOIN [dbo].[mObjectCategory] C ON O.CategoryId=C.Id WHERE O.ObjectId={ledgerId} AND C.RoleTypeId=1146 AND C.RoleId={oldGroupId.GetValueOrDefault()}");
            }
        }
        public static async Task MapLedgerToDefaultRoleClass(this IRepositoryAsync<Ledger> repository, long ledgerId, long? newRoleId, long? oldRoleId)
        {
            if (newRoleId > 0)
            {
                var clsRepo = repository.GetRepository<ObjectClassMap>();
                var ctgQuery = repository.GetRepository<ObjectCategory>().Queryable().Where(x =>
                    x.RoleId == newRoleId && x.RoleTypeId == 1145 && x.Objects.All(y => y.ObjectId != ledgerId));
                if (await ctgQuery.AnyAsync())
                {
                    var query = ctgQuery.SelectMany(x => x.ObjectClasses)
                        .Where(x => x.ClassName == "All").Select(x => new
                        {
                            ClassId = x.Id,
                            CategoryId = x.CategoryId,
                            x.ClassName
                        });
                    if (query.Any())
                    {
                        var cls = await query.ToListAsync();
                        var list = cls.Select(x => new ObjectClassMap
                        {
                            Id = 0,
                            ObjectState = ObjectState.Added,
                            ObjectId = ledgerId,
                            ClassId = x.ClassId,
                            CategoryId = x.CategoryId
                        }).ToList();
                        clsRepo.InsertRange(list);//Insert for new group
                        await repository.UOW.SaveChangesAsync();
                    }
                }
                
            }
            
            if (newRoleId != oldRoleId && oldRoleId > 0)
            {
                await repository.ExecuteSqlAsync(
                    $"DELETE O FROM [dbo].[tObjectClassMap] AS O JOIN [dbo].[mObjectCategory] C ON O.CategoryId=C.Id WHERE O.ObjectId={ledgerId} AND C.RoleTypeId=1145 AND C.RoleId={oldRoleId.GetValueOrDefault()}");
            }
        }
        

        public static IQueryable<vwAccountLedger> GetAllAccountLedger(
            this IRepository<Ledger> repository,
            DateTime FromDate, DateTime ToDate, long accountId,long? officeId)
        {
            var fy = repository.GetRepository<FinancialYear>().Queryable().Where(
                            x => x.OpeningDate <= FromDate && x.ClosingDate >= FromDate).Select(x=>x.Id).FirstOrDefault();
            
            if (fy == 0) return new List<vwAccountLedger>().AsQueryable();
            var frmDate = FromDate.ToString("dd/MMM/yyyy");
            var toDate = FromDate.ToString("dd/MMM/yyyy");
            var vdrepo = repository.GetRepository<VoucherDetail>().Queryable();

            var opdata =
                (from t in
                    vdrepo.Where(
                        x =>
                            x.Voucher.VoucherDate < FromDate.Date && x.Voucher.IsAccepted &&
                            x.Voucher.IsAccountsVisiblity && x.AccountId == accountId && x.Voucher.FinancialYearId == fy)
                    select new
                    {
                        t.Amount
                    }).ToList();


            var openingBal = new List<vwAccountLedger>
            {
                new vwAccountLedger
                {
                    AccountId = accountId,
                    Particulars = "Opening As of:  " + frmDate,
                    VoucherDate = null,
                    Debit = opdata.Sum(x => x.Amount)>0?opdata.Sum(x => x.Amount):0,
                    Credit = opdata.Sum(x => x.Amount) < 0?opdata.Sum(x => x.Amount)*-1:0
                }
            };

            var logsData =
                (from t in
                    vdrepo.Where(
                        x =>
                                x.Voucher.VoucherDate >= FromDate.Date && x.Voucher.VoucherDate <= ToDate.Date && x.AccountId == accountId && x.Voucher.FinancialYearId == fy)
                select new vwAccountLedger
                {
                    VoucherId = t.VoucherId,
                    AccountId=t.AccountId,
                    VdId = t.Id,
                    VOfficeId = t.Voucher.OfficeId,
                    VdOfficeId = t.OfficeId,
                    VoucherDate = t.Voucher.VoucherDate,
                    Office = t.fk_Office.OfficeName,
                    VoucherNo = t.Voucher.VoucherNo,
                    ChequeNo = t.ChequeNo,
                    VoucherType = t.Voucher.FK_VoucherType.VoucherTypeName,
                    Particulars = t.Voucher.Account3Id > 0
                        ? "as per details"
                        : (t.Voucher.Account3Id == null && t.Voucher.Account1Id == t.AccountId
                            ? t.Voucher.Account2.AccountName
                            : t.Voucher.Account1.AccountName),
                    Debit = t.Amount > 0 ? t.Amount : 0,
                    Credit = t.Amount > 0 ? 0 : t.Amount*-1,
                    Narration=t.Narration
                }).ToList();
            var transactionTotal = new vwAccountLedger
            {
                AccountId = accountId,
                Particulars = "Transaction Total: ",
                VoucherDate = null,
                Debit = logsData.Sum(x => x.Debit),
                Credit = logsData.Sum(x => x.Credit)

            };
            
            var udata = openingBal.Union(logsData).ToList();
            var clamount = udata.Sum(x => x.Debit) - udata.Sum(x => x.Credit);

            var closingBal = new vwAccountLedger
            {
                AccountId = accountId,
                Particulars = "Closing As of:   " + toDate,
                VoucherDate = null,
                Debit = clamount > 0 ? clamount : 0,
                Credit = clamount < 0 ? clamount*-1 : 0,

            };
            udata.Add(transactionTotal);
            udata.Add(closingBal);

            return udata.AsQueryable();
        }

        public static IQueryable<vwDayBook> GetAllDayBook(
            this IRepository<Ledger> repository,
            DateTime FromDate,string classIds, string accountIds, long categoryId)
        {
            var fy = repository.GetRepository<FinancialYear>().Queryable().Where(
                            x => x.OpeningDate <= FromDate.Date && x.ClosingDate >= FromDate.Date).Select(x => x.Id).FirstOrDefault();

            if (fy == 0) return new List<vwDayBook>().AsQueryable();
            //var frmDate = FromDate.Date;
            
            var objectids = repository.GetRepository<ObjectClassMap>()
                .GetObjectClassMap(classIds, categoryId, accountIds);

            var vdrepo = repository.GetRepository<VoucherDetail>().Queryable();
            

            var ldata =
                (from o in objectids join t in
                    vdrepo.Where(
                        x =>
                                x.Voucher.VoucherDate == FromDate.Date && x.Voucher.IsAccepted && x.Voucher.IsAccountsVisiblity) on o.ObjectId equals  t.OfficeId
                 select new vwDayBook
                 {
                     VoucherDate = t.Voucher.VoucherDate,
                     Particulars = t.Voucher.Account3Id > 0
                         ? "as per details"
                         : (t.Voucher.Account3Id == null && t.Voucher.Account1Id == t.AccountId
                             ? t.Voucher.Account2.AccountName
                             : t.Voucher.Account1.AccountName),
                     VoucherNo = t.Voucher.VoucherNo,
                     ChequeNo = t.ChequeNo,
                     VoucherType = t.Voucher.FK_VoucherType.VoucherTypeName,
                     
                     Debit = t.Amount > 0 ? t.Amount : 0,
                     Credit = t.Amount > 0 ? 0 : t.Amount * -1,
                     Narration = t.Narration
                 }).ToList();
            
            return ldata.AsQueryable();
        }
    }
}
