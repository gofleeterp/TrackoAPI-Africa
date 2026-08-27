using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.FMS.Dues;

namespace TrackoAPI.Repository
{
    public static class DueTransactionLogRepository
    {

        public static IQueryable<DueTransactionLog> GetDueTransactionLogsByDueTypeId(this IRepository<DueTransactionLog> repository,long id) => repository.Queryable().Where(x => id == x.DueTypeId);

        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is null.</exception>
        public static vwDueVoucher GetBulkEntryByVoucherId(this IRepository<DueTransactionLog> repository, long key)
        {
            try
            {
                var voucher = repository.GetRepository<Voucher>().Queryable().Where(x => x.Id == key).Select(x => new vwDueVoucher()
                {
                    OfficeId = x.OfficeId,
                    Narration = x.UserRemark,
                    DueAccountId = x.Account1Id.Value,
                    DueAccountName = x.Account1.AccountName,
                    DueAmount = x.Amount1,
                    PayableAccountId = x.Account2Id.Value,
                    PayableAccountName = x.Account2.AccountName,
                    PayableAmount = x.Amount2,
                    OtherAccountId = x.Account3Id,
                    OtherAccountName = x.Account3Id == null ? "" : x.Account3.AccountName,
                    OtherAmount = x.Amount3,
                    OthPayableAccountId = x.Account4Id,
                    OthPayableAccountName = x.Account4Id == null ? "" : x.Account4.AccountName,
                    OthPayableAmount = x.Amount4,
                    PaidDate = x.VoucherDate,
                    Id = x.Id,
                    DocumentNo = x.VoucherNo,
                    PaidAmount = x.VoucherAmount,
                    OfficeName = x.fk_Office.OfficeName,
                    IsLocked = (x.IsAudited || x.IsAccepted) && x.IsAccountsVisiblity,
                    ChequeDate = x.VoucherDate,
                    PageId = x.PageId,

                    IGSTAccountId = x.Account5Id,
                    IGSTAccountName = x.Account5Id == null ? "" : x.Account5.AccountName,
                    IGSTAmount = x.Amount5,

                    CGSTAccountId = x.Account6Id,
                    CGSTAccountName = x.Account6Id == null ? "" : x.Account6.AccountName,
                    CGSTAmount = x.Amount6,

                    SGSTAccountId = x.Account7Id,
                    SGSTAccountName = x.Account7Id == null ? "" : x.Account7.AccountName,
                    SGSTAmount = x.Amount7,
                    CurRate = x.CurRate,
                    CurTypeId = x.CurTypeId,
                    ConstCurTypeId = x.ConstCurTypeId
                }).FirstOrDefault();
                if (voucher == null)
                {
                    return null;
                }
                voucher.DueLogs.AddRange(repository.Queryable()
                    .Where(x => x.VoucherId == key)
                    .Select(x => new vwDueTransactionLog()
                    {
                        RefNo1 = x.RefNo1,
                        RefNo2 = x.RefNo2,
                        Id = x.Id,
                        DueTypeId = x.DueTypeId,
                        DueTypeName = x.fk_DueType.Name,
                        DueAmount = x.DueAmount,
                        OtherAmount = x.OtherAmount,
                        MiscCharg=x.MiscCharge,
                        VehicleId = x.VehicleId,
                        VehicleNo = x.fk_Vehicle.VehicleNo,
                        Remark = x.Remark,
                        ExpiryDate = x.ExpiryDate,
                        OwnerId = x.OwnerId,
                        OwnerName = x.OwnerId == null ? "" : x.fk_Owner.AccountName,
                        StartDate = x.StartDate,
                        IGSTPAmount = x.IGSTPAmount,
                        IGSTPAmountP = x.IGSTPAmountP,

                        CGSTPAmount = x.CGSTPAmount,
                        CGSTPAmountP = x.CGSTPAmountP,

                        SGSTPAmount = x.SGSTPAmount,
                        SGSTPAmountP = x.SGSTPAmountP,

                        IGSTTPAmount = x.IGSTTPAmount,
                        IGSTTPAmountP = x.IGSTTPAmountP,

                        CGSTTPAmount = x.CGSTTPAmount,
                        CGSTTPAmountP = x.CGSTTPAmountP,

                        SGSTTPAmount = x.SGSTTPAmount,
                        SGSTTPAmountP = x.SGSTTPAmountP,
                        InsuranceLog = x.fk_InsuranceLog == null ? null : new vwDueInsuranceLog()
                        {
                            Id = x.fk_InsuranceLog.Id,
                            AgentName = x.fk_InsuranceLog.AgentName,
                            Compulsory = x.fk_InsuranceLog.Compulsory,
                            Discount = x.fk_InsuranceLog.Discount,
                            GVWOD = x.fk_InsuranceLog.GVWOD,
                            ImposedValue = x.fk_InsuranceLog.ImposedValue,
                            InsCompanyId = x.fk_InsuranceLog.InsCompanyId,
                            InsCompanyName = x.fk_InsuranceLog.InsCompanyId == null ? "" : x.fk_InsuranceLog.fk_InsuranceCompany.AccountName,
                            InsOfficerName = x.fk_InsuranceLog.InsOfficerName,
                            InsuredValue = x.fk_InsuranceLog.InsuredValue,
                            IsComprehensive = x.fk_InsuranceLog.IsComprehensive,
                            NCBAmount = x.fk_InsuranceLog.NCBAmount,
                            NCBPercent = x.fk_InsuranceLog.NCBPercent,
                            PACCount = x.fk_InsuranceLog.PACCount,
                            PACValue = x.fk_InsuranceLog.PACValue,
                            Premium = x.fk_InsuranceLog.Premium,
                            TPPremium = x.fk_InsuranceLog.TPPremium
                        }
                    }));
                return voucher;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
    }
}
