using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoAPI.ViewModels.FMS.Battery;

namespace TrackoAPI.Repository
{
    public static class BatteryLogRepository
    {
        public static BatteryLog GetLastBatteryLogByStatusAndLife(this IRepositoryAsync<BatteryLog> repository, long BatteryId,
           long[] voucherTypes, int life,long currentLogId)
       {
           return
               repository.Queryable().Where(x => x.BatteryId == BatteryId && x.BatteryLife == life && voucherTypes.Contains(x.VoucherTypeId) &&x.Id< currentLogId).OrderByDescending(x=>x.Id).FirstOrDefault();
       }
        public static IQueryable<BatteryLog> GetAllBatteryLogList(this IRepository<BatteryLog> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }

        public static vwBatteryBillView GetBatteryClaimBillView(this IRepositoryAsync<BatteryLog> repository, long key)
       {
           var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
           {
               Id = tei.Id,
               OfficeId = tei.OfficeId.Value,
               OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
               DocumentDate = tei.DocDate,
               DocumentNo = tei.DocNo,
               PrimaryDebitAccountId = tei.DrAccountId,
               PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
               PrimaryCreditAccountId = tei.CrAccountId.Value,
               PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
               //OtherLedgerId = tei.fk_Voucher.Account3Id,
               //OtherLedgerName = (tei.fk_Voucher.Account3 == null ? null : tei.fk_Voucher.Account3.AccountName),
               Narration = tei.Remark
           }).FirstOrDefault(x => x.Id == key);
           if (record == null)
               throw new BusinessException(ErrorCode.GLB109, $"Requested Battery Claim / Refurbish transaction not found");
           var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
           {
               x.TSLId,
               Id = x.Id,
               BatteryId = x.BatteryId,
               BatterySerialNo = x.BatterySerialNo,
               BrandId = x.fk_Battery.BrandId,
               BrandName = x.fk_Battery.fk_Brand == null ? null : x.fk_Battery.fk_Brand.BrandName,
               ReferenceId = x.PreviousLogId,
               Remark = x.Remark,
               RowVersion = x.RowVersion,
               DocNo = x.fk_PreviousLog.DocNo,
               DocDate = x.fk_PreviousLog == null ? (DateTime?) null : x.fk_PreviousLog.DocDate,
               CreditAc =
                   x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName

           }).ToList();

           var Batterylogs = logs.Select(x => new vwBatteryClaimLog
           {
              TSLId = x.TSLId,
               RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
               DocDate = x.DocDate.Value,
               CreditAc = x.CreditAc,
               DocNo= x.DocNo,
               Id = x.Id,
               BatteryId = x.BatteryId,
               BatterySerialNo = x.BatterySerialNo,
               BrandId = x.BrandId,
               BrandName = x.BrandName,
               Remark = x.Remark,
               ReferenceId = (long) x.ReferenceId
           });

           record.ClaimLog = Batterylogs.ToList();
           return record;
       }

        public static vwBatteryBillView GetBatteryResaleBillView(this IRepositoryAsync<BatteryLog> repository, long key)
        {
            var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.DocDate,
                DocumentNo = tei.DocNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                //OtherLedgerId = tei.fk_Voucher.Account3Id,
                //OtherLedgerName = (tei.fk_Voucher.Account3 == null ? null : tei.fk_Voucher.Account3.AccountName),
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if(record==null)throw new BusinessException(ErrorCode.GLB109,$"Requested Battery Resale transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x=>x.ExtraInfoId==record.Id).Select(x => new 
            {
                x.TSLId,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.fk_Battery.BrandId,
                BrandName = x.fk_Battery.fk_Brand==null?null:x.fk_Battery.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                PurchaseAmount = x.Rate,
                OtherAmt = x.OtherAmount,
                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                BillNo = x.fk_PreviousLog.DocNo,
                PurchaseDate = x.fk_PreviousLog==null?(DateTime?)null:x.fk_PreviousLog.DocDate,
                SupplierName = x.fk_PreviousLog.fk_CreditAccount==null?null:x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();
            
            var Batterylogs = logs.Select(x => new vwBatteryResaleLog
            {
                TSLId = x.TSLId,
                BillNo = x.BillNo,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                PurchaseDate = x.PurchaseDate,
                SupplierName = x.SupplierName,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long) x.ReferenceId,
                PurchaseAmount = x.PurchaseAmount,
                OtherAmt = x.OtherAmt,
                NetValue = x.NetValue
            });

            record.ResaleLog = Batterylogs.ToList();
            return record;
        }
        public static vwBatteryChassisBill GetChassisBillView(this IRepositoryAsync<BatteryLog> repository, long key)
       {
           var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x=>new
           {
               x.Id,
               x.CreditAccountId,
               CreditAccount=x.fk_CreditAccount.AccountName,
               x.DebitAccountId,
               DebitAccount=x.fk_DebitAccount.AccountName,
               x.NetAmount,
               x.VehicleId,
               x.fk_Vehicle.VehicleNo,
               x.BatteryAge,
               x.Remark,
               x.DocDate,
               x.RowVersion,
               x.BatteryId,
               x.BatterySerialNo,
               x.fk_Battery.BrandId,
               BrandName=x.fk_Battery.fk_Brand!=null? x.fk_Battery.fk_Brand.BrandName:null,
               x.NextLogId,
               x.TSLId
           }).ToList();
           var first = logs.First();
           if (first == null) return null;
           var record=new vwBatteryChassisBill()
           {
               StoreId = first.CreditAccountId,
               IssueDate = first.DocDate,
               DocumentNumber = first.VehicleNo,
               EstimatedTotalAmt = logs.Sum(x=>x.NetAmount),
               StoreName = first.CreditAccount
           };
           var Batterylogs = logs.Select(x => new vwBatteryLog()
           {
               TSLId = x.TSLId,
               Id=x.Id,
               VehicleId = x.VehicleId,
               VehicleNo = x.VehicleNo,
               Remark = x.Remark,
               BrandId = x.BrandId,
               BatteryAge = x.BatteryAge,
               NetAmount = x.NetAmount,
               OwnerId = x.DebitAccountId,
               OwnerName = x.DebitAccount,
               RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
               BrandName = x.BrandName,
               ReferenceId = x.NextLogId,
               BatteryId = x.BatteryId,
               BatterySerialNo = x.BatterySerialNo
           });
           record.BatteryLogs= Batterylogs.ToList();
           return record;
       }
        public static vwBatteryBillView GeBatteryBillPurchaseView(this IRepositoryAsync<BatteryLog> repository,long id,long type)
        {
            var tei = repository.GetRepository<BatteryLogExtraInfo>().Find(id);
            if(tei==null)return new vwBatteryBillView();
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
            var v = new vwBatteryBillView
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
                CGSTLedgerId = voucher.CGSTLedgerId,
                CGSTLedgerName = voucher.CGSTLedgerName,
                
                ProvisionalAcId=tei.ProvisionalAcId,
                ProvisionalAcName=tei.ProvisionalAcId>0?tei.fk_ProvisionalAc.AccountName:null,

                SGSTLedgerId = voucher.SGSTLedgerId,
                SGSTLedgerName = voucher.SGSTLedgerName,

                IGSTLedgerId = voucher.IGSTLedgerId,
                IGSTLedgerName = voucher.IGSTLedgerName,
                CGSTAmount = voucher.CGSTAmount,
                SGSTAmount = voucher.SGSTAmount,
                IGSTAmount = voucher.IGSTAmount,
                //OtherLedgerId = voucher.OtherLedgerId,
                //OtherLedgerName = voucher.OtherLedgerName,
                //OtherAmount = voucher.OtherAmount,
                Narration = voucher.Narration,
                OfficeId = voucher.OfficeId,
                OfficeName = voucher.OfficeName,
                VoucherTypeId = voucher.VoucherTypeId,
                RowVersion_Id = Encoding.UTF8.GetString(voucher.RowVersion),
                CalVat = tei.CalVat,
                //CalOthAmt = tei.CalOthAmt,
                VendorReferenceNo = tei.VendorReferenceNo
            };
            //var extraInfo = repository.GetRepository<BatteryLogExtraInfo>().Queryable().FirstOrDefault(x => x.Id == id);
            List<vwBatteryLog> list =
                repository.Queryable()
                    .Include(x => x.fk_Battery.fk_Brand)
                    .Where(x => x.ExtraInfoId == tei.Id)
                    .Select(x => new vwBatteryLog()
                    {
                        TSLId=x.TSLId,
                        Id = x.Id,
                        OtherAmount = x.OtherAmount,
                        VehicleId = x.VehicleId,
                        Remark = x.Remark,
                        VehicleNo = x.fk_Vehicle != null ? x.fk_Vehicle.VehicleNo : null,
                        BatteryId = x.BatteryId,
                        BatterySerialNo = x.BatterySerialNo,
                        CGSTAmount = x.CGSTAmount,
                        SGSTAmount = x.SGSTAmount,
                        IGSTAmount = x.IGSTAmount,
                        ServiceTaxTypeId=x.TaxServiceTypeId,
                        Rate = x.Rate,
                        SubTotal = x.SubTotal,
                        DiscountAmount = x.DiscountAmount,
                        DiscountPercent = x.DiscountPercent,
                        BrandId = x.fk_Battery.BrandId,
                        BrandName = x.fk_Battery.fk_Brand != null ? x.fk_Battery.fk_Brand.BrandName : null,
                        RoundAmount=x.RoundAmount,
                        NetAmount = x.NetAmount,
                        //PurchaseOrderId = x.PurchaseOrderId,
                        CGSTPercent = x.CGSTPercent,
                        SGSTPercent = x.SGSTPercent,
                        IGSTPercent = x.IGSTPercent,
                        WarrantyDays = x.WarrantyDays,
                ReferenceId = x.PreviousLogId,
                ReferenceBatterySerialNo = x.fk_PreviousLog != null ? x.fk_PreviousLog.BatterySerialNo : null,//added by sanjay
                CarriedCost = x.TransferPrice,//added by sanjay
                //PurchaseOrderNo = x.fk_PurchaseOrder != null ? x.fk_PurchaseOrder.PONo : null,
                JobCardId = x.JobsheetId,
                JobCardNo = x.fk_Jobsheet.TriplogNo,
                MechanicId = x.MechanicId,
                Mechanic = x.fk_Mechanic.Name
            }).ToList();
            v.Batterys = list;
            return v;
       }
        public static vwBatteryBillView GetBatteryScrapBillView(this IRepositoryAsync<BatteryLog> repository, long key)
        {
            var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.DocDate,
                DocumentNo = tei.DocNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,//StoreId
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName), //StoreName
                //OtherLedgerId = tei.fk_Voucher.Account2Id.Value,//Income Id
                //OtherLedgerName = (tei.fk_Voucher.Account2 == null ? null : tei.fk_Voucher.Account2.AccountName), //Income a/c name
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null) throw new BusinessException(ErrorCode.GLB109, $"Requested Battery Scrap transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.fk_Battery.BrandId,
                BrandName = x.fk_Battery.fk_Brand == null ? null : x.fk_Battery.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                DocDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.DocDate,
                CreditAc = x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();

            var Batterylogs = logs.Select(x => new vwBatteryScrapLog
            {
                TSLId = x.TSLId,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId,
                BatteryCost = x.NetValue,
                ReceivedDate = x.DocDate,
                ReceivedFrom = x.CreditAc
            });
            record.ScrapLog = Batterylogs.ToList();
            return record;
        }
        public static vwBatteryBillView GetBatteryStoretransferOutBillView(this IRepositoryAsync<BatteryLog> repository, long key)
        {
            var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.DocDate,
                DocumentNo = tei.DocNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                ProvisionalAcId = tei.ProvisionalAcId,
                ProvisionalAcName = tei.ProvisionalAcId > 0 ? tei.fk_ProvisionalAc.AccountName : null,
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null) throw new BusinessException(ErrorCode.GLB109, $"Requested Battery transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.fk_Battery.BrandId,
                BrandName = x.fk_Battery.fk_Brand == null ? null : x.fk_Battery.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                PurchaseAmount = x.Rate,

                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                ReceiptNo = x.fk_PreviousLog.DocNo,
                ReceivedDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.DocDate,
                ReceivedFrom = x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();

            var Batterylogs = logs.Select(x => new vwBatteryStoreTransferLog
            {
                TSLId = x.TSLId,

                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                ReceiptNo=x.ReceiptNo,
                ReceivedDate = x.ReceivedDate,
                ReceivedFrom = x.ReceivedFrom,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId,
                BatteryCost = x.NetValue
            });

            record.StoreTransferLog = Batterylogs.ToList();
            return record;
        }
        public static vwBatteryBillView GetBatteryStoretransferInBillView(this IRepositoryAsync<BatteryLog> repository, long key)
        {
            var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.DocDate,
                DocumentNo = tei.DocNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                ProvisionalAcId = tei.ProvisionalAcId,
                ProvisionalAcName = tei.ProvisionalAcId > 0 ? tei.fk_ProvisionalAc.AccountName : null,
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null) throw new BusinessException(ErrorCode.GLB109, $"Requested Battery transaction not found");
            // var logs = repository.Queryable().Where(x => x.ExtraInfoId == key).Select(x => new
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.fk_Battery.BrandId,
                BrandName = x.fk_Battery.fk_Brand == null ? null : x.fk_Battery.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                PurchaseAmount = x.Rate,

                NetValue = x.NetAmount,
                RowVersion = x.RowVersion,
                ReceiptNo = x.fk_PreviousLog.DocNo,
                ReceivedDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.DocDate,
                ReceivedFrom = x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName
            }).ToList();

            var Batterylogs = logs.Select(x => new vwBatteryStoreTransferLog
            {TSLId = x.TSLId,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                ReceiptNo = x.ReceiptNo,
                ReceivedDate = x.ReceivedDate,
                ReceivedFrom = x.ReceivedFrom,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId,
                BatteryCost = x.NetValue
            });

            record.StoreTransferLog = Batterylogs.ToList();
            return record;
        }
        public static vwBatteryBillView GetBatteryRejectBillView(this IRepositoryAsync<BatteryLog> repository, long key)
        {
            var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.DocDate,
                DocumentNo = tei.DocNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null)
                throw new BusinessException(ErrorCode.GLB109, $"Requested Battery transaction not found");
            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {x.TSLId,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.fk_Battery.BrandId,
                BrandName = x.fk_Battery.fk_Brand == null ? null : x.fk_Battery.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                RowVersion = x.RowVersion,
                SendDocNo = x.fk_PreviousLog.DocNo,
                SendDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.DocDate,
                SenderStore =
                    x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName

            }).ToList();

            var Batterylogs = logs.Select(x => new vwBatteryRejectLog
            {
                TSLId = x.TSLId,
                SendDocNo = x.SendDocNo,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                SendDate = x.SendDate.Value,
                SenderStore = x.SenderStore,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                ReferenceId = (long)x.ReferenceId
            });

            record.RejectLog = Batterylogs.ToList();
            return record;
        }
        public static vwBatteryBillView GetBatteryRefurbishReceiptBillView(this IRepositoryAsync<BatteryLog> repository, long key)
        {
            var record = repository.GetRepository<BatteryLogExtraInfo>().Queryable().Select(tei => new vwBatteryBillView
            {
                Id = tei.Id,
                OfficeId = tei.OfficeId.Value,
                OfficeName = (tei.fk_Office == null ? null : tei.fk_Office.OfficeName),
                DocumentDate = tei.DocDate,
                DocumentNo = tei.DocNo,
                PrimaryDebitAccountId = tei.DrAccountId,
                PrimaryDebitAccountName = (tei.fk_DrAccount == null ? null : tei.fk_DrAccount.FleetAcName),
                PrimaryCreditAccountId = tei.CrAccountId.Value,
                PrimaryCreditAccountName = (tei.fk_CrAccount == null ? null : tei.fk_CrAccount.FleetAcName),
                PrimaryDebitAmount =(tei.fk_Voucher == null ? 0 : tei.fk_Voucher.Amount1),
                Narration = tei.Remark
            }).FirstOrDefault(x => x.Id == key);
            if (record == null)
                throw new BusinessException(ErrorCode.GLB109, $"Requested Battery transaction not found");

            var logs = repository.Query(x => x.ExtraInfoId == record.Id).Select(x => new
            {
                x.TSLId,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.fk_Battery.BrandId,
                BrandName = x.fk_Battery.fk_Brand == null ? null : x.fk_Battery.fk_Brand.BrandName,
                ReferenceId = x.PreviousLogId,
                Remark = x.Remark,
                RowVersion = x.RowVersion,
                BatteryCost=x.NetAmount,
                CarriedCost=x.TransferPrice,//added by sanjay
                RoundAmount=x.RoundAmount,
                SendDocNo = x.fk_PreviousLog.DocNo,
                SendDate = x.fk_PreviousLog == null ? (DateTime?)null : x.fk_PreviousLog.DocDate,
                SenderStore =
                    x.fk_PreviousLog.fk_CreditAccount == null ? null : x.fk_PreviousLog.fk_CreditAccount.FleetAcName

            }).ToList();

            var Batterylogs = logs.Select(x => new vwBatteryRefurbishReceiptLog
            {
                TSLId = x.TSLId,
                SendDocNo = x.SendDocNo,
                RowVersionId = Encoding.UTF8.GetString(x.RowVersion),
                SendDate = x.SendDate.Value,
                SenderStore = x.SenderStore,
                Id = x.Id,
                BatteryId = x.BatteryId,
                BatterySerialNo = x.BatterySerialNo,
                BrandId = x.BrandId,
                BrandName = x.BrandName,
                Remark = x.Remark,
                CarriedCost= x.CarriedCost,//added by sanjay
                BatteryCost = x.BatteryCost,
                RoundAmount=x.RoundAmount,
                ReferenceId = (long)x.ReferenceId
            });

            record.RefurbishReceiptLog = Batterylogs.ToList();
            return record;
        }

       
    }
}
