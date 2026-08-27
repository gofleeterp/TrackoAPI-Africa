using System;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoAPI.ViewModels.FMS.Repairs;

namespace TrackoAPI.Repository
{
    public static class SpareLogRepository
    {
        public static IQueryable<SpareLog> GetAllSpareLogList(this IRepository<SpareLog> repository, long id) => repository.Queryable().Where(x => id == x.Id);
        public static vwSparePurchaseBill GetPurchaseBillView(this IRepository<SpareLog> repository, long id,long type)
        {
            var vchRepo = repository.GetRepository<Voucher>().Queryable();
            var v = vchRepo.Where(x => x.Id == id&&x.VoucherTypeId== type).Select(x => new vwSparePurchaseBill()
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

                SGSTLedgerId = x.Account4Id,
                SGSTLedgerName = x.Account4 != null ? x.Account4.AccountName : null,
                SGSTAmount = x.Amount4,

                IGSTLedgerId = x.Account7Id,
                IGSTLedgerName = x.Account7 != null ? x.Account7.AccountName : null,
                IGSTAmount = x.Amount7,

                OtherLedgerId = x.Account5Id,
                OtherLedgerName = x.Account5 != null ? x.Account5.AccountName : null,
                OtherAmount = x.Amount5,

                ExpenseAmount2 = (decimal)x.Amount6,
                ExpenseLedgerId2 = x.Account6Id,
                ExpenseLedger2Name = x.Account6 != null ? x.Account6.AccountName : null,

                Narration = x.UserRemark,
                OfficeId = x.OfficeId,
                OfficeName = x.fk_Office.OfficeName,

                VoucherTypeId = x.VoucherTypeId,
                PageId = x.PageId
            }).FirstOrDefault();
            if (v == null) return null;
            var extraInfo = repository.GetRepository<SpareLogExtraInfo>().Queryable().FirstOrDefault(x => x.Id == v.Id);
            if (extraInfo != null)
            {
                v.ORMId = extraInfo.OrmId;
                v.CalVat = extraInfo.CalculateVat;
                v.OtherChgRatioId = extraInfo.OtherChargeRatioId;
                v.OtherChgRatio = extraInfo.OtherChargeRatio?.ConstantName;
                v.VendorReferenceNo = extraInfo.VendorReferenceNo;
                v.TypeId = extraInfo.TypeId.GetValueOrDefault(0);
                v.TypeName = extraInfo.fk_Type?.ConstantName;
            }
            var spareLogs = repository.Queryable().Where(x => x.VoucherId == id).Select(x => new vwSpareLog()
            {
                Id = x.Id,
                Amount = x.Amount,
                TSLId=x.TSLId,
                OtherAmount = x.OtherAmount,
                VehicleId = x.VehicleId,
                Remark = x.Remark,
                Qty = x.Qty,
                VehicleNo = x.fk_Vehicle != null ? x.fk_Vehicle.VehicleNo : null,

                HireVehicleId=x.HireVehicleId,
                HireVehicleNo = x.fk_HireVehicle != null ? x.fk_HireVehicle.RegistrationNo : null,

                SpareId = x.SparePartId,
                SpareName = x.fk_Spare.SpareName,
                CGSTAmount =  x.CGSTAmount, //x.VatAmount,
                CGSTRate = x.CGSTRate,//x.VatPercent,
                SGSTAmount = x.SGSTAmount,
                SGSTRate = x.SGSTRate,
                IGSTAmount = x.IGSTAmount,
                IGSTRate = x.IGSTRate,
                Rate = x.Rate,
                UnitId = x.UnitId,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                DiscountPercent = x.DiscountPercent,
                MakeId = x.MakeId,
                NetAmount = x.NetAmount,
                PurchaseId = x.POLogId,
                WarrantyDays = x.WarrantyDays,
                WarrantyKm = x.WarrantyKm,
                ODOKm=x.ODOKm,
                ReferenceId = x.ReferenceId,
                //PurchaseNo = x.fk_PurchaseOrder==null?null:x.fk_PurchaseOrder.PONo,
                MakeName = x.fk_Make.Name,
                JobCardId = x.JobCardId,
                MechanicId = x.MechanicId,
                FittingPositionId = x.FittingPositionId,
                FittingPositionName = x.fk_FittingPosition!=null?x.fk_FittingPosition.Name:null,
                JobCardNo = x.fk_JobCard!=null? x.fk_JobCard.TriplogNo:null,
                Mechanic = x.fk_Mechanic!=null?x.fk_Mechanic.Name:null,
                StockTransferDate = x.VoucherTypeId==26?x.fk_Reference.VoucherDate:(DateTime?) null,
                VoucherNo= x.VoucherTypeId == 26 ? x.fk_Reference.VoucherNo : null,
                TransferedQty = x.VoucherTypeId==26?x.fk_Reference.Qty:(decimal?) null,
                UnitTypeId = x.UnitTypeId,
                UnitType = x.UnitTypeId > 0 ? x.fk_UnitType.ConstantName : null
            });
            if (spareLogs.Any())
            {
                v.Spares = spareLogs.ToList();
                if (v.VoucherTypeId == 25|| v.VoucherTypeId == 26)
                {
                    if (v.VoucherTypeId == 25)
                    {
                        var store =
                    repository.Queryable()
                        .Where(x => x.VoucherId == v.Id)
                        .Select(x => new { x.DrAccountId, x.fk_DrAccount.AccountName })
                        .FirstOrDefault();
                        v.PrimaryDebitAccountId = store.DrAccountId.GetValueOrDefault(0);
                        v.PrimaryDebitAccountName = store.AccountName;
                    }
                    if (v.VoucherTypeId == 26)
                    {
                        var store =
                    repository.Queryable()
                        .Where(x => x.VoucherId == v.Id)
                        .Select(x => new { OtherAccountId = x.CrAccountId, x.fk_CrAccount.AccountName })
                        .FirstOrDefault();
                        v.PrimaryCreditAccountId = store.OtherAccountId.GetValueOrDefault(0);
                        v.PrimaryCreditAccountName = store.AccountName;
                    }
                }
            }
            var labourLogs =
                repository.GetRepository<RepairLabourLog>()
                    .Queryable()
                    .Where(x => x.VoucherId == v.Id)
                    .Select(x => new vwLabourLog()
                    {
                        Id = x.Id,
                        TSLId=x.TSLId,
                        Amount = x.Amount,
                        OtherAmount = x.OtherAmount,
                        Remark = x.Remark,
                        LaborQty = x.LaborQty,
                        VehicleId = x.VehicleId,
                        VehicleNo = x.fk_Vehicle != null ? x.fk_Vehicle.VehicleNo : null,
                        
                        HireVehicleId = x.HireVehicleId,
                        HireVehicleNo = x.fk_HireVehicle != null ? x.fk_HireVehicle.RegistrationNo : null,

                        SubTotal = x.SubTotal,
                        DiscountAmount = x.DiscountAmount,
                        DiscountPercent = x.DiscountPercent,
                        NetAmount = x.NetAmount,
                        WorkOrderId = x.POLogId,
                        //ServiceTaxPercent = x.ServiceTaxPercent,
                        //ServiceTaxAmount = x.ServiceTaxAmount,

                        LCCGSTPercent = x.CGSTPercent,
                        LCCGSTAmount = x.CGSTAmount,

                        LCSGSTPercent = x.SGSTPercent,
                        LCSGSTAmount = x.SGSTAmount,

                        LCIGSTPercent = x.IGSTPercent,
                        LCIGSTAmount = x.IGSTAmount,
                        ODOKm=x.ODOKm,
                        LaborId = x.LaborId,
                        LaborName = x.fk_Labor != null ? x.fk_Labor.SpareName : null,
                        LaborRate = x.LaborRate,
                        LaborUnitId = x.LaborUnitId,
                        LaborUnitName = x.fk_LaborUnit != null ? x.fk_LaborUnit.UnitName : null,
                        MechanicId = x.MechanicId,
                        MechanicName = x.fk_Mechanic != null ? x.fk_Mechanic.Name : null,
                        WorkOrderNo = x.fk_POLog != null ? x.fk_POLog.fk_PurchaseOrder.PONo : null,
                        JobCardId = x.JobCardId,
                        JobCardNo = x.fk_JobCard != null ? x.fk_JobCard.TriplogNo : null
                    });
            if (labourLogs.Any())
            {
                v.Labors = labourLogs.ToList();
            }
            return v;
        }
    }

}
