using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;
using TrackoAPI.ViewModels.Global;

namespace TrackoAPI.ViewModels.FMS.Repairs
{
    [EdmComplexType]
    public class vwSparePurchaseBill
    {
        public vwSparePurchaseBill()
        {
            JsonData = new List<JsonDataEntity>();
        }
        public long Id { get; set; }
        public long? VoucherTypeId { get; set; }
        public long? TypeId { get; set; }
        public string TypeName { get; set; }
        [Required]
        public long OfficeId { get; set; }
        [Required]
        public DateTime DocumentDate { get; set; }
        [MaxLength(50)]
        [Required]
        public string DocumentNo { get; set; }
        [Required]
        public long PrimaryCreditAccountId { get; set; }//2
        public string PrimaryCreditAccountName { get; set; }
        public decimal PrimaryCreditAmount { get; set; } = 0;//2
        [MaxLength(50)]
        public string VendorReferenceNo { get; set; }

        public long? ORMId { get; set; }
        public string ORMNo { get; set; }
        public string Narration { get; set; }
        public long? PrimaryDebitAccountId { get; set; }//1
        public string PrimaryDebitAccountName { get; set; }
        public decimal PrimaryDebitAmount { get; set; } = 0;//2

        public string ProvisionalAccountName { get; set; }
        public long? ProvisionalAcId { get; set; }//1


        public long? ExpenseLedgerId2 { get; set; }//1
        public string ExpenseLedger2Name { get; set; }
        public decimal ExpenseAmount2 { get; set; } = 0;//2
        public long? CGSTLedgerId { get; set; }//3
        public string CGSTLedgerName { get; set; }
        public long? SGSTLedgerId { get; set; }//3
        public string SGSTLedgerName { get; set; }
        public long? IGSTLedgerId { get; set; }//3
        public string IGSTLedgerName { get; set; }
        public long? TDSLedgerId { get; set; }//3
        public string TDSLedgerName { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal TDSRate { get; set; }
        public long? TdsNatureId { get; set; }

        //        public decimal VatAmount { get; set; } = 0;//3
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;
//        public long? ServiceTaxLedgerId { get; set; }//4
//       public string ServiceTaxLedgerName { get; set; }
//        public decimal ServiceTaxAmount { get; set; } = 0;//4
        public long? OtherLedgerId { get; set; }//5
        public string OtherLedgerName { get; set; }
        public decimal OtherAmount { get; set; } = 0;//5
        /// <summary>
        /// If <c>true</c> GST or Taxes will be saved seperatly in Credit Side.
        /// If <c>false</c> GST or Taxes will included in Expense Head in Credit Side
        /// </summary>
        public bool CalVat { get; set; } = false;
        public long? OtherChgRatioId { get; set; }
        public string OfficeName { get; set; }
        public List<vwSpareLog> Spares { get; set; }
        public List<vwLabourLog> Labors { get; set; }
        public List<vwVehiclePm> VehiclePm { get; set; }
        public string OtherChgRatio { get; set; }
        public long? PageId { get; set; }
        public string DocumentNumber { get; set; }
        public string GatepassNo { get; set; }
        public string GatepassType { get; set; }
        public long? GPVehicleId { get; set; }
        public string ChallanSlipNo { get; set; }
        public DateTime? ChallanSlipDate { get; set; }
        public long? ViewId { get; set; }
        public long? VehicleId { get; set; }
        public long? HireVehicleId { get; set; }
        public string BatchId { get; set; }
        public List<JsonDataEntity> JsonData { get; set; }
        public long? TCSAccountId { get; set; }
        public string TCSAccountName { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal TCSRate { get; set; }
        public decimal CGSTPercent { get; set; }
        public decimal SGSTPercent { get; set; }
        public decimal IGSTPercent { get; set; }
        public decimal Qty { get; set; }
        public long? GroupVoucherId { get; set; }
        public long? RelatedVoucherId { get; set; }
        public long? OtherChargeRatioId { get; set; }
        public long? PostDiscountAcId { get; set; }
        public decimal PostDiscAmount { get; set; }
        public long? RoundOffAcId { get; set; }
        public string RoundOffAc { get; set; }
        public decimal RoundOff { get; set; } = 0;
        public long? TDSVoucherId { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
        public long? DestnCountryId { get; set; }
        public string DestnCountry { get; set; }
    }
    [EdmComplexType]
    public class vwSpareLog
    {
        public vwSpareLog()
        {
            JsonData = new List<JsonDataEntity>();
        }
        public long Id { get; set; }
        public long? TSLId { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }

        public long? HireVehicleId { get; set; }
        public string HireVehicleNo { get; set; }

        [Required]
        public long SpareId { get; set; }
        [Required]
        public string SpareName { get; set; }
        public long? MakeId { get; set; }
        public long? BinId { get; set; }
        public string MakeName { get; set; }
        public long? PurchaseId { get; set; }
        public string PurchaseNo { get; set; }
        public int WarrantyKm { get; set; } = 0;
        public int ODOKm { get; set; } = 0;
        public int WarrantyDays { get; set; } = 0;
        public decimal Qty { get; set; } = 0;
        public decimal DepositedQty { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        //public decimal VatPercent { get; set; } = 0;
        //public decimal VatAmount { get; set; } = 0;
        //lokesh
        public long? TaxServiceTypeId { get; set; }
        public string TaxServiceType { get; set; }
        public decimal CGSTRate { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTRate { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTRate { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;
        //lokesh

        public decimal SubTotal { get; set; } = 0;
        public decimal OtherAmount { get; set; } = 0;
        public decimal PostDisount { get; set; } = 0;
        public decimal RoundOff { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        public string Remark { get; set; }
        public long? ReferenceId { get; set; }
        public long? FittingPositionId { get; set; }
        public string FittingPositionName { get; set; }
        public long? JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public long? MechanicId { get; set; }
        public string Mechanic { get; set; }
        //used for VoucherType 26 Only
        public DateTime? StockTransferDate { get; set; }
        public decimal? TransferedQty { get; set; }

        public string VoucherNo { get; set; }
        public long? ExtraInfoId { get; set; }
        public long? UnitId { get; set; }
        public List<JsonDataEntity> JsonData { get; set; }
        public decimal StockQty { get; set; }
        public long? VoucherId { get; set; }
        public long? UnitTypeId { get; set; }
        public string UnitType { get; set; }
        public string Category { get; set; }
    }

    [EdmComplexType]
    public class vwLabourLog
    {
        public long Id { get; set; }
        public long? TSLId { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? HireVehicleId { get; set; }
        public string HireVehicleNo { get; set; }

        public long? WorkOrderId { get; set; }
        public string WorkOrderNo { get; set; }

        [Required]
        public long LaborId { get; set; }

        [Required]
        public string LaborName { get; set; }

        public long? MechanicId { get; set; }
        public string MechanicName { get; set; }
        public decimal LaborQty { get; set; } = 0;
        public long? LaborUnitId { get; set; }
        public string LaborUnitName { get; set; }
        public decimal LaborRate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        //public decimal ServiceTaxPercent { get; set; } = 0;
        //public decimal ServiceTaxAmount { get; set; } = 0;

        public long? GSTServiceTypeId { get; set; }
        public decimal LCCGSTPercent { get; set; } = 0;
        public decimal LCCGSTAmount { get; set; } = 0;

        public decimal LCSGSTPercent { get; set; } = 0;
        public decimal LCSGSTAmount { get; set; } = 0;

        public decimal LCIGSTPercent { get; set; } = 0;
        public decimal LCIGSTAmount { get; set; } = 0;
        public int ODOKm { get; set; } = 0;
        public decimal SubTotal { get; set; } = 0;
        public decimal OtherAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        public string Remark { get; set; }
        public long? ExtraInfoId { get; set; }
        public long? JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public string _JsonData { get; set; }
        public List<JsonDataEntity> JsonData { get; set; }
        public decimal Value1 { get; set; }
        public decimal Value2 { get; set; }
        public long? PurchaseOrderId { get; set; }
    }
    [EdmComplexType]
    public class vwPreventiveMaintance
    {
        public long Id { get; set; }

        [Required]
        public long PMId { get; set; }

        public string PMName { get; set; }
        public long PMChartId { get; set; }
        public bool IsRemoved { get; set; }
    }
    [EdmComplexType]
    public class vwVehiclePm
    {
        public long Id { get; set; }
        public long PMId { get; set; }
        public long ScheduleId { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long ClassId { get; set; }
        public long? JobCardId { get; set; }
        public long? BillId { get; set; }
        public long? NewPMId { get; set; }
        public long? NextLogId { get; set; }
        public long? PreviousLogId { get; set; }
        public DateTime JobDate { get; set; }
        public int StartKM { get; set; } = 0;
        public DateTime DueDate { get; set; }
        public DateTime? DueAlertDate { get; set; }
        public int DueKM { get; set; }
        public int DueDays { get; set; }
        public int AlertKM { get; set; }
        public int AlertDays { get; set; }
    }
}
