using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.Models.Shared;

namespace TrackoAPI.Repository
{
   public static class GeneralExpenseLogRepository
    {
        public static IQueryable<GeneralExpenseLog> GetAllGeneralExpenseLogList(this IRepository<GeneralExpenseLog> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
        public static vwGeneralExpenseVoucher GetBulkEntryByVoucherId(this IRepository<GeneralExpenseLog> repository, long key)
        {
            //var status= Helper.GetFinanceStatus()==FinanceStatus.ApprovalRequired;
            var voucher = repository.GetRepository<Voucher>().Queryable().Where(x => x.Id == key).Select(x => new vwGeneralExpenseVoucher()
            {
                OfficeId = x.OfficeId,
                Remark = x.UserRemark,
                CrAccountId = x.Account2Id.Value,
                CrAccountName = x.Account2.AccountName,
                DocumentDate = x.VoucherDate,
                Id = x.Id,
                DocumentNo = x.VoucherNo,
                DrAccountId = x.Account1Id.Value,
                DrAccountName = x.Account1.AccountName,
                NetAmount = x.VoucherAmount,
                OfficeName = x.fk_Office.OfficeName,
                IsLocked = x.IsAccountsVisiblity&&x.IsAudited,
                PageId = x.PageId,
                ViewId=x.ViewId
            }).FirstOrDefault();
            if (voucher == null)
            {
                return null;
            }
            var expenses = repository.Queryable()
                .Where(x => x.VoucherId == key)
                .Select(x => new vwGeneralExpenseLog()
                {
                    VoucherNo = x.VoucherNo,
                    Amount = x.Amount,
                    VoucherId = x.VoucherId,
                    ExpenseDate = x.ExpenseDate,
                    OfficeId = x.OfficeId,
                    CreditAccountId = x.CreditAccountId,
                    VehicleId = x.VehicleId,
                    DriverId = x.DriverId ?? 0,
                    ExpenseNatureId=x.ExpenseNatureId,
                    ReferenceNo = x.ReferenceNo,
                    DebitAccountId = x.DebitAccountId,
                    Remark = x.Remark,
                    ExpenseId = x.Id,
                    CreditAccount = x.fk_CreditAccount.AccountName,
                    DebitAccountName = x.fk_DebitAccount.AccountName,
                    DriverName = x.fk_Driver.DriverName,
                    OfficeName = x.fk_Office.OfficeName,
                    VehicleNo = x.fk_Vehicle.VehicleNo,
                    PaidInId = x.PaidInId,
                    PaidIn = x.fk_PaidIn==null?null:x.fk_PaidIn.ConstantName,
                    Ref1 = x.Ref1,
                    ViewId=x.ViewId

                });
            voucher.GeneralExpenseLogs = new List<vwGeneralExpenseLog>(expenses);
            return voucher;
        }
    }
}
