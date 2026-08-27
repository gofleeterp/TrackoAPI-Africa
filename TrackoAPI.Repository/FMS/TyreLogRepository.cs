using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.FMS.Repairs;
using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.Reports.ViewModels.FMS.Tyre;

namespace TrackoAPI.Repository
{
   public static class TyreLogRepository
    {
        public static TyreLog GetLastTyreLogByStatusAndLife(this IRepositoryAsync<TyreLog> repository, long tyreId,
           long voucherType, int life,long currentLogId)
       {
           return
               repository.Queryable().Where(x => x.TyreId == tyreId && x.TyreLife == life && x.VoucherTypeId == voucherType&&x.Id< currentLogId).OrderByDescending(x=>x.Id).FirstOrDefault();
       }
        public static IQueryable<TyreLog> GetAllTyreLogList(this IRepository<TyreLog> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }

        public static vwTyreBillView GetTyreClaimBillView(this IRepositoryAsync<TyreLog> repository, long key)
       {
           var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
           {
               Id = tei.Id,
               OfficeId = tei.OfficeId.Value,
               OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
               DocumentDate = tei.VoucherDate,
               DocumentNo = tei.VoucherNo,
               PrimaryDebitAccountId = tei.DrAccountId,
               PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
               PrimaryCreditAccountId = tei.CrAccountId.Value,
               PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
               //OtherLedgerId = tei.fk_Voucher.Account3Id,
               //OtherLedgerName = (tei.fk_Voucher.Account3 == null ? null : tei.fk_Voucher.Account3.AccountName),
               Narration = tei.Remark,
               PageId = tei.PageId
           }).FirstOrDefault(x => x.Id == key);
           if (record == null)
               throw new BusinessException(ErrorCode.GLB109, $"Requested Tyre Claim / Remould transaction not found");
           var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
           {
               Id = x.Id,
               TSLId=x.TSLId,
               TyreId = x.TyreId,
               TyreNo = x.TyreNo,
               BrandId = x.fk_Tyre.BrandId,
               BrandName = x.fk_Tyre.fk_Brand == null ? null : x.fk_Tyre.fk_Brand.BrandName,
               ReferenceId = x.PreviousLogId,
               Remark = x.Remark,
               RowVersion = x.RowVersion,
               DocNo = x.fk_PreviousLog.VoucherNo,
               DocDate = x.fk_PreviousLog == null ? (DateTime?) null : x.fk_PreviousLog.VoucherDate,
               CreditAc =
                   x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName

           }).ToList();

           var tyrelogs = logs.Select(x => new vwTyreClaimLog
           {
               //BillNo = x.BillNo,
               RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
               DocDate = x.DocDate.Value,
               CreditAc = x.CreditAc,
               DocNo= x.DocNo,
               Id = x.Id,
               TSLId = x.TSLId,
               TyreId = x.TyreId,
               TyreNo = x.TyreNo,
               BrandId = x.BrandId,
               BrandName = x.BrandName,
               Remark = x.Remark,
               ReferenceId = (long) x.ReferenceId
           });

           record.ClaimLog = tyrelogs.ToList();
           return record;
       }

        public static vwTyreBillView GetTyreResaleBillView(this IRepositoryAsync<TyreLog> repository, long key)
        {
            var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.VoucherDate,
                DocumentNo = tei.VoucherNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                OtherLedgerId = tei.fk_Voucher.Account3Id,
                OtherLedgerName = (tei.fk_Voucher.Account3 == null ? null : tei.fk_Voucher.Account3.AccountName),
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if(record==null)throw new BusinessException(ErrorCode.GLB109,$"Requested Tyre Resale transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x=>x.ExtraInfoId==record.Id).Select(x => new 
            {
                x.TSLId,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.fk_Tyre.BrandId,
                BrandName = x.fk_Tyre.fk_Brand==null?null:x.fk_Tyre.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                PurchaseAmount = x.Rate,
                OtherAmt = x.OtherAmount,
                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                BillNo = x.fk_PreviousLog.VoucherNo,
                PurchaseDate = x.fk_PreviousLog==null?(DateTime?)null:x.fk_PreviousLog.VoucherDate,
                SupplierName = x.fk_PreviousLog.fk_CreditAccount==null?null:x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();
            
            var tyrelogs = logs.Select(x => new vwTyreResaleLog
            {
                BillNo = x.BillNo,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                PurchaseDate = x.PurchaseDate,
                SupplierName = x.SupplierName,
                Id = x.Id,
                TSLId= x.TSLId,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long) x.ReferenceId,
                PurchaseAmount = x.PurchaseAmount,
                OtherAmt = x.OtherAmt,
                NetValue = x.NetValue
            });

            record.ResaleLog = tyrelogs.ToList();
            return record;
        }
        public static vwTyreChassisBill GetChassisBillView(this IRepositoryAsync<TyreLog> repository, long key)
       {
           var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x=>new
           {
               x.TSLId,
               x.Id,
               x.CreditAccountId,
               CreditAccount=x.fk_CreditAccount.AccountName,
               x.DebitAccountId,
               DebitAccount=x.fk_DebitAccount.AccountName,
               x.NetAmount,
               x.VehicleId,
               x.fk_Vehicle.VehicleNo,
               x.IsStepney,
               x.KmReading,
               x.Remark,
               x.VoucherDate,
               x.RowVersion,
               x.TyreId,
               x.TyreNo,
               x.AirPressure,
               x.fk_Tyre.BrandId,
               BrandName=x.fk_Tyre.fk_Brand!=null? x.fk_Tyre.fk_Brand.BrandName:null,
               x.fk_Tyre.ProdMonth,
               WPName= (x.fk_TyreCheck!=null&& x.fk_TyreCheck.fk_WheelPosition!=null)? x.fk_TyreCheck.fk_WheelPosition.Name : null,
               WPId= x.fk_TyreCheck != null?x.fk_TyreCheck.WheelPositionId:null,
               x.NextLogId,
               x.ExtraInfoId,
               x.VoucherNo
           }).ToList();
           var first = logs.First();
           if (first == null) return null;
           var record=new vwTyreChassisBill()
           {
               StoreId = first.CreditAccountId,
               IssueDate = first.VoucherDate,
               DocumentNumber = first.VoucherNo,
               Id = first.ExtraInfoId.Value,
               EstimatedTotalAmt = logs.Sum(x=>x.NetAmount),
               StoreName = first.CreditAccount
           };
           var tyrelogs = logs.Select(x => new vwTyreLog()
           {
               Id=x.Id,
               TSLId=x.TSLId,
               VehicleId = x.VehicleId,
               VehicleNo = x.VehicleNo,
               Remark = x.Remark,
               AirPressure = x.AirPressure,
               BrandId = x.BrandId,
               IsStepney = x.IsStepney,
               KmReading = x.KmReading,
               NetAmount = x.NetAmount,
               OwnerId = x.DebitAccountId,
               OwnerName = x.DebitAccount,
               ProductionMonth = x.ProdMonth,
               RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
               BrandName = x.BrandName,
               ReferenceId = x.NextLogId,
               TyreId = x.TyreId,
               TyreNo = x.TyreNo,
               WheelPositionId = x.WPId,
               WheelPositionName = x.WPName,
               KmRun = 0
           });
           record.TyreLogs= tyrelogs.ToList();
           return record;
       }
        public static vwTyreBillView GeTyreBillPurchaseView(this IRepositoryAsync<TyreLog> repository,long id,long type)
        {
            var tei = repository.GetRepository<TyreLogExtraInfo>().Find(id);
            if(tei==null)return new vwTyreBillView();
            var vchRepo = repository.GetRepository<Voucher>().Queryable();
            var voucher = vchRepo.Where(x => x.Id == tei.VoucherId && x.VoucherTypeId == type).Select(x => new
            {
                Id = x.Id,
                DocumentDate = x.VoucherDate,
                DocumentNo = x.VoucherNo,

                PrimaryDebitAmount = x.Amount1,
                PrimaryDebitAccountId = x.Account1Id,
                PrimaryDebitAccountName = x.Account1 != null ? x.Account1.AccountName : null,

                

                PrimaryCreditAccountId = x.Account2Id.Value,
                PrimaryCreditAccountName = x.Account2 != null ? x.Account2.AccountName : null,
                PrimaryCreditAmount = x.Amount2,

                CGSTLedgerId = x.Account3Id,
                CGSTLedgerName = x.Account3 != null ? x.Account3.AccountName : null,
                CGSTAmount = x.Amount3,

                OtherLedgerId = x.Account4Id,
                OtherLedgerName = x.Account4 != null ? x.Account4.AccountName : null,
                OtherAmount = x.Amount4,

                SGSTLedgerId = x.Account5Id,
                SGSTLedgerName = x.Account5 != null ? x.Account5.AccountName : null,
                SGSTAmount = x.Amount5,

                IGSTLedgerId = x.Account6Id,
                IGSTLedgerName = x.Account6 != null ? x.Account6.AccountName : null,
                IGSTAmount = x.Amount6,

                Narration = x.UserRemark,
                OfficeId = x.OfficeId,
                OfficeName = x.fk_Office.OfficeName,

                VoucherTypeId = x.VoucherTypeId,
                x.RowVersion
            }).FirstOrDefault();
            if (voucher == null) return null;
            var v = new vwTyreBillView
            {
                Id = voucher.Id,
                DocumentDate = voucher.DocumentDate,
                DocumentNo = voucher.DocumentNo,
                PrimaryDebitAmount = voucher.PrimaryDebitAmount,
                PrimaryDebitAccountId = voucher.PrimaryDebitAccountId,
                PrimaryDebitAccountName = voucher.PrimaryDebitAccountName,
                PrimaryCreditAccountId = voucher.PrimaryCreditAccountId,
                PrimaryCreditAccountName = voucher.PrimaryCreditAccountName,
                PrimaryCreditAmount = voucher.PrimaryCreditAmount,

                ProvisionalAcId = tei.ProvisionalAcId,
                ProvisionalAcName = tei.ProvisionalAcId>0?tei.fk_ProvisionalAc.AccountName:null,

                CGSTLedgerId = voucher.CGSTLedgerId,
                //CGSTLedgerName = voucher.CGSTLedgerName,
                CGSTAmount = voucher.CGSTAmount,

                SGSTLedgerId = voucher.SGSTLedgerId,
                //SGSTLedgerName = voucher.SGSTLedgerName,
                SGSTAmount = voucher.SGSTAmount,

                IGSTLedgerId = voucher.IGSTLedgerId,
                //IGSTLedgerName = voucher.IGSTLedgerName,
                IGSTAmount = voucher.IGSTAmount,

                OtherLedgerId = voucher.OtherLedgerId,
                OtherLedgerName = voucher.OtherLedgerName,
                OtherAmount = voucher.OtherAmount,
                Narration = voucher.Narration,
                OfficeId = voucher.OfficeId,
                OfficeName = voucher.OfficeName,
                VoucherTypeId = voucher.VoucherTypeId,
                RowVersion_Id = Encoding.UTF8.GetString(voucher.RowVersion),
                CalVat = tei.CalVat,
                CalOthAmt = tei.CalOthAmt,
                VendorReferenceNo = tei.VendorReferenceNo
            };
            //var extraInfo = repository.GetRepository<TyreLogExtraInfo>().Queryable().FirstOrDefault(x => x.Id == id);
            List<vwTyreLog> list =
                repository.Queryable()
                    .Include(x => x.fk_Tyre.fk_Brand)
                    .Where(x => x.ExtraInfoId == tei.Id)
                    .Select(x => new vwTyreLog()
                    {
                        TSLId = x.TSLId,
                        Id = x.Id,
                        OtherAmount = x.OtherAmount,
                        VehicleId = x.VehicleId,
                        Remark = x.Remark,
                        VehicleNo = x.fk_Vehicle != null ? x.fk_Vehicle.VehicleNo : null,
                        TyreId = x.TyreId,
                        TyreNo = x.TyreNo,
                        CGSTAmount = x.CGSTAmount,
                        CGSTPercent = x.CGSTPercent,
                        SGSTAmount = x.SGSTAmount,
                        SGSTPercent = x.SGSTPercent,
                        IGSTAmount = x.IGSTAmount,
                        IGSTPercent = x.IGSTPercent,
                        Rate = x.Rate,
                        SubTotal = x.SubTotal,
                        DiscountAmount = x.DiscountAmount,
                        DiscountPercent = x.DiscountPercent,
                        BrandId = x.fk_Tyre.BrandId,
                        BrandName = x.fk_Tyre.fk_Brand != null ? x.fk_Tyre.fk_Brand.BrandName : null,
                        NetAmount = x.NetAmount,
                        //PurchaseId = x.PurchaseOrderId,
                        WarrantyDays = x.WarrantyDays,
                        WarrantyKm = x.WarrantyKm,
                        ReferenceId = x.PreviousLogId,
                        ReferenceTyreNo = x.fk_PreviousLog != null ? x.fk_PreviousLog.TyreNo : null,//added by sanjay
                        CarriedCost = x.TransferPrice,//added by sanjay
                                                      //PurchaseNo = x.fk_PurchaseOrder != null ? x.fk_PurchaseOrder.PONo : null,
                        JobCardId = x.JobsheetId,
                        JobCardNo = x.fk_Jobsheet.TriplogNo,
                        MechanicId = x.MechanicId,
                        Mechanic = x.fk_Mechanic.Name,
                        ProductionMonth = x.fk_Tyre.ProdMonth
                    }).ToList();
            v.Tyres = list;
            return v;
       }
        public static vwTyreBillView GetTyreScrapBillView(this IRepositoryAsync<TyreLog> repository, long key)
        {
            var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.VoucherDate,
                DocumentNo = tei.VoucherNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,//StoreId
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName), //StoreName
                OtherLedgerId = tei.fk_Voucher.Account2Id.Value,//Income Id
                OtherLedgerName = (tei.fk_Voucher.Account2 == null ? null : tei.fk_Voucher.Account2.AccountName), //Income a/c name
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null) throw new BusinessException(ErrorCode.GLB109, $"Requested Tyre Scrap transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.fk_Tyre.BrandId,
                BrandName = x.fk_Tyre.fk_Brand == null ? null : x.fk_Tyre.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                TyreCost=x.Rate,
                CGSTAmount = x.CGSTAmount,
                CGSTPercent = x.CGSTPercent,
                SGSTAmount = x.SGSTAmount,
                SGSTPercent = x.SGSTPercent,
                IGSTAmount = x.IGSTAmount,
                IGSTPercent = x.IGSTPercent,
                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                DocDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.VoucherDate,
                CreditAc = x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();

            var tyrelogs = logs.Select(x => new vwTyreScrapLog
            {
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId,
                TyreCost = x.NetValue,
                ReceivedDate = x.DocDate,
                ReceivedFrom = x.CreditAc,
                TSLId=x.TSLId
            });
            record.ScrapLog = tyrelogs.ToList();
            return record;
        }
        public static vwTyreBillView GetTyreStoretransferOutBillView(this IRepositoryAsync<TyreLog> repository, long key)
        {
            var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.VoucherDate,
                DocumentNo = tei.VoucherNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                ProvisionalAcId = tei.ProvisionalAcId,
                ProvisionalAcName = tei.ProvisionalAcId > 0 ? tei.fk_ProvisionalAc.AccountName : null,

                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null) throw new BusinessException(ErrorCode.GLB109, $"Requested Tyre transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.fk_Tyre.BrandId,
                BrandName = x.fk_Tyre.fk_Brand == null ? null : x.fk_Tyre.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                PurchaseAmount = x.Rate,

                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                ReceiptNo = x.fk_PreviousLog.VoucherNo,
                ReceivedDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.VoucherDate,
                ReceivedFrom = x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();

            var tyrelogs = logs.Select(x => new vwTyreStoreTransferLog
            {
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                ReceiptNo=x.ReceiptNo,
                ReceivedDate = x.ReceivedDate,
                ReceivedFrom = x.ReceivedFrom,
                Id = x.Id,
                TSLId = x. TSLId,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId,
                TyreCost = x.NetValue
            });

            record.StoreTransferLog = tyrelogs.ToList();
            return record;
        }
        public static vwTyreBillView GetTyreStoretransferInBillView(this IRepositoryAsync<TyreLog> repository, long key)
        {
            var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.VoucherDate,
                DocumentNo = tei.VoucherNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                ProvisionalAcId = tei.ProvisionalAcId,
                ProvisionalAcName = tei.ProvisionalAcId > 0 ? tei.fk_ProvisionalAc.AccountName : null,

                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null) throw new BusinessException(ErrorCode.GLB109, $"Requested Tyre transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.fk_Tyre.BrandId,
                BrandName = x.fk_Tyre.fk_Brand == null ? null : x.fk_Tyre.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                PurchaseAmount = x.Rate,

                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                ReceiptNo = x.fk_PreviousLog.VoucherNo,
                ReceivedDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.VoucherDate,
                ReceivedFrom = x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();

            var tyrelogs = logs.Select(x => new vwTyreStoreTransferLog
            {
                TSLId = x.TSLId,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                ReceiptNo = x.ReceiptNo,
                ReceivedDate = x.ReceivedDate,
                ReceivedFrom = x.ReceivedFrom,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId,
                TyreCost = x.NetValue
            });

            record.StoreTransferLog = tyrelogs.ToList();
            return record;
        }
        public static vwTyreBillView GetTyreRejectBillView(this IRepositoryAsync<TyreLog> repository, long key)
        {
            var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.VoucherDate,
                DocumentNo = tei.VoucherNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null)
                throw new BusinessException(ErrorCode.GLB109, $"Requested Tyre transaction not found");
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
               x.TSLId,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.fk_Tyre.BrandId,
                BrandName = x.fk_Tyre.fk_Brand == null ? null : x.fk_Tyre.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                RowVersion = x.RowVersion,
                SendDocNo = x.fk_PreviousLog.VoucherNo,
                SendDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.VoucherDate,
                SenderStore =
                    x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName

            }).ToList();

            var tyrelogs = logs.Select(x => new vwTyreRejectLog
            {
                TSLId = x.TSLId,
                SendDocNo = x.SendDocNo,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                SendDate = x.SendDate.Value,
                SenderStore = x.SenderStore,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId
            });

            record.RejectLog = tyrelogs.ToList();
            return record;
        }
        public static vwTyreBillView GetTyreRemouldReceiptBillView(this IRepositoryAsync<TyreLog> repository, long key)
        {
            var record = repository.GetRepository<TyreLogExtraInfo>().Queryable().Select(tei => new vwTyreBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.VoucherDate,
                DocumentNo = tei.VoucherNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),

                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null)
                throw new BusinessException(ErrorCode.GLB109, $"Requested Tyre transaction not found");

            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                BrandId = x.fk_Tyre.BrandId,
                BrandName = x.fk_Tyre.fk_Brand == null ? null : x.fk_Tyre.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                RowVersion = x.RowVersion,
                rate = x.Rate,
                CGSTPercentage = x.CGSTPercent,
                CGSTAmount = x.CGSTAmount,
                SGSTPercentage = x.SGSTPercent,
                SGSTAmount = x.SGSTAmount,
                IGSTPercentage = x.IGSTPercent,
                IGSTAmount = x.IGSTAmount,


                TyreCost = x.NetAmount,
                CarriedCost = x.TransferPrice,//added by sanjay
                SendDocNo = x.fk_PreviousLog.VoucherNo,
                SendDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.VoucherDate,
                SenderStore =
                    x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName,
                x.RubberTypeId,
                RubberType = x.fk_RubberType == null ? null : x.fk_RubberType.BrandName
            }).ToList();

            var tyrelogs = logs.Select(x => new vwTyreRemouldReceiptLog
            {
                TSLId=x.TSLId,
                SendDocNo = x.SendDocNo,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                SendDate = x.SendDate.Value,
                SenderStore = x.SenderStore,
                Id = x.Id,
                TyreId = x.TyreId,
                TyreNo = x.TyreNo,
                RubberTypeId=x.RubberTypeId,
                RubberType=x.RubberType,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                Amount = x.rate,
                CGSTPercentage = x.CGSTPercentage,
                CGSTAmount = x.CGSTAmount,
                SGSTPercentage = x.SGSTPercentage,
                SGSTAmount = x.SGSTAmount,
                IGSTPercentage = x.IGSTPercentage,
                IGSTAmount = x.IGSTAmount,
                CarriedCost = x.CarriedCost,//added by sanjay
                TyreCost = x.TyreCost,
                ReferenceId = (long)x.ReferenceId
            });

            record.RemouldReceiptLog = tyrelogs.ToList();
            return record;
        }

       public static IQueryable<TyreLog> GetReportData(this IRepositoryAsync<TyreLog> repository, string classIds, string accountIds, long categoryId,string ledgerFilterType)
       {
            var objectids = repository.GetRepository<ObjectClassMap>()
                 .GetObjectsForReport(classIds, categoryId, accountIds);
            switch (ledgerFilterType)
            {
                case "debit":
                    return repository.Queryable().Where(x => objectids.Item2.Contains(x.DebitAccountId)).AsQueryable();
                case "credit":
                    return repository.Queryable().Where(x => objectids.Item2.Contains(x.CreditAccountId)).AsQueryable();
                case "vehicle":
                    return repository.Queryable().Where(x => x.VehicleId.HasValue && objectids.Item2.Contains(x.VehicleId.Value)).AsQueryable();
                case "office":
                    return repository.Queryable().Where(x => objectids.Item2.Contains(x.ExtraInfo.OfficeId.Value)).AsQueryable();
            }
            return repository.Queryable();
        }

        
    }

    

}
