using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;

namespace TrackoAPI.ViewModels.FMS.Battery
{
    [EdmComplexType]
    public class vwBatteryBillView
    {
        public long? ProvisionalAcId { get; set; }
        public string ProvisionalAcName { get; set; }
        public long Id { get; set; }
        public long? ReceiptId { get; set; }

        public long? VoucherTypeId { get; set; }
        
        [Required]
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        [Required]
        public DateTime DocumentDate { get; set; }

        [MaxLength(50)]
        [Required]
        public string DocumentNo { get; set; }

        [Required]
        public long PrimaryCreditAccountId { get; set; } //2
        public string PrimaryCreditAccountName { get; set; }
        public decimal PrimaryCreditAmount { get; set; } = 0; //2

        [MaxLength(50)]
        public string VendorReferenceNo { get; set; }
        public string Narration { get; set; }
        public long? PrimaryDebitAccountId { get; set; } //1
        public string PrimaryDebitAccountName { get; set; }
        public decimal PrimaryDebitAmount { get; set; } = 0; //2
        public long? ServiceTaxTypeId { get; set; }
        public string ServiceTaxType { get; set; }
        public long? CGSTLedgerId { get; set; } //3
        public string CGSTLedgerName { get; set; }

        public long? SGSTLedgerId { get; set; } //3
        public string SGSTLedgerName { get; set; }

        public long? IGSTLedgerId { get; set; } //3
        public string IGSTLedgerName { get; set; }

        public decimal CGSTAmount { get; set; } = 0; //3
        public decimal SGSTAmount { get; set; } = 0; //3
        public decimal IGSTAmount { get; set; } = 0; //3
        public long? OtherLedgerId { get; set; } //5
        public string OtherLedgerName { get; set; }
        public decimal OtherAmount { get; set; } = 0; //5
        public bool CalVat { get; set; } = false;
        public bool CalOthAmt { get; set; } = false;
        
        public long? OtherChgRatioId { get; set; }
        public string OtherChgRatio { get; set; }
        public List<vwBatteryLog> Batterys { get; set; }
        public List<vwBatteryIssueReceipt> IssueReceiptLogs { get; set; }
        public List<vwBatteryResaleLog> ResaleLog { get; set; }
        public List<vwBatteryClaimLog> ClaimLog { get; set; }
        public List<vwBatteryScrapLog> ScrapLog { get; set; }
        public List<vwBatteryStoreTransferLog> StoreTransferLog { get; set; }
        public List<vwBatteryRejectLog> RejectLog { get; set; }
        public List<vwBatteryRefurbishReceiptLog> RefurbishReceiptLog { get; set; }
        public List<vwBatteryClaimSettlementLog> BatteryClaimSettlementLog { get; set; }
        public List<vwBatteryIssueLog> IssueLogs { get; set; }
        public List<vwBatteryReceiptLog> ReceiptLogs { get; set; }
        public string RowVersion_Id { get; set; }
        public string RowVersion_ReceiptId { get; set; }
        public long? PageId { get; set; }
        public long? ViewId { get; set; }
        public long? TCSAccountId { get; set; }
        public string TCSAccountName { get; set; }
        public decimal TCSRate { get; set; }
        public decimal TCSAmount { get; set; }
        public long? RoundOffAcId { get; set; }
        public decimal RoundOffAmount { get; set; }
        public long? PostDiscountAcId { get; set; }
        public decimal PostDiscountAmount { get; set; }
        public vwBatteryBillView()
        {
            Batterys=new List<vwBatteryLog>();
            IssueReceiptLogs=new List<vwBatteryIssueReceipt>();
            ResaleLog=new List<vwBatteryResaleLog>();
        }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
    }

    [EdmComplexType]
    public class vwBatteryLog
    {
        public long Id { get; set; }
        public long? VehicleId { get; set; }
        public long? TSLId { get; set; }
        public string VehicleNo { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long? BrandId { get; set; }
        public string BrandName { get; set; }
        public long? PurchaseExtraInfoId { get; set; }
        public string PurchaseExtraInfoNo { get; set; }
        public int WarrantyDays { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public long? ServiceTaxTypeId { get; set; }
        public string ServiceTaxType { get; set; }
        public decimal CGSTPercent { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTPercent { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTPercent { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        public decimal SubTotal { get; set; } = 0;
        public decimal OtherAmount { get; set; } = 0;
        public decimal RoundAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        public string Remark { get; set; }
        public long? ReferenceId { get; set; }
        public string ReferenceBatterySerialNo { get; set; }//added by sanjay
        public decimal CarriedCost { get; set; }//added by sanjay
        public long? NextUsedId { get; set; }
        public string NextUsed { get; set; }
        public long? JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public long? MechanicId { get; set; }
        public string Mechanic { get; set; }
        public long? ReceiptId { get; set; }
        public string ReceiptNo { get; set; }
        public long? ReceiptBatteryId { get; set; }
        public string ReceiptBatterySerialNo { get; set; }
        public int? BatteryAge { get; set; }
        public string RowVersionId { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int?  GravityLevel { get; set; }
        public long? PurchaseOrderId { get; set; }
        public string PurchaseOrderNo { get; set; }
        public DateTime? LogDate { get; set; }
    }
    [EdmComplexType]
    public class vwBatteryIssueReceipt
    {
        public long? TSLId { get; set; }
        public long IssueLogId { get; set; }
        public string IssueBatterySerialNo { get; set; }
        public long IssueBatteryId { get; set; }
        public long? IssueReferenceId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }

        public int? GravityLevel { get; set; }
        public bool IsWaterLevelChecked { get; set; }
        public bool IsTerminalCarbonChecked { get; set; }
       
        public decimal IssueAmount { get; set; }
        public long? JobSheetId { get; set; }
        public string JobSheetNo { get; set; }
        public string IssueRemark { get; set; }
        public long? MechanicId { get; set; }
        public string MechanicName { get; set; }
        public string IssueRowVersionId { get; set; }
        public long ReceiptLogId { get; set; }
        public long ReceiptBatteryId { get; set; }
        public string ReceiptBatterySerialNo { get; set; }
        public DateTime? ReceiptOnDate { get; set; }
        public int? ReceiptAge { get; set; } = 0;
        public decimal ReceiptAmount { get; set; } = 0;
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public long NextUseId { get; set; }
        public string NextUseName { get; set; }
        public string ReceiptRemark { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public long? ReceiptReferenceId { get; set; }
        public string ReceiptRowVersionId { get; set; }
        
        public long? NextLogId { get; set; }
    }

    [EdmComplexType]
    public class vwBatteryChassisBill
    {
        public long Id { get; set; } = 0;
        [MaxLength(100)]
        public string DocumentNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public long StoreId { get; set; }
        [MaxLength(200)]
        public string StoreName { get; set; }
        public long OfficeId { get; set; }
        [MaxLength(200)]
        public string OfficeName { get; set; }

        public decimal EstimatedTotalAmt { get; set; }

        public List<vwBatteryLog> BatteryLogs { get; set; }
        public long? PageId { get; set; }
        public long? ViewId { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
    }

    [EdmComplexType]
    public class vwBatteryResaleLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string BillNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal OtherAmt { get; set; }
        public decimal NetValue { get; set; }
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
    }
    [EdmComplexType]
    public class vwBatteryClaimLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public long ReferenceId { get; set; }
        public DateTime DocDate { get; set; }
        public string CreditAc { get; set; }
        public string DocNo { get; set; }
        public string Remark { get; set; }
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string RowVersionId { get; set; }
    }
    [EdmComplexType]
    public class vwBatteryScrapLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string ReceiptNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReceivedFrom { get; set; }
        public decimal BatteryCost { get; set; }
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
    }
    public class vwBatteryStoreTransferLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string ReceiptNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReceivedFrom { get; set; }
        public decimal BatteryCost { get; set; }
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
    }
    public class vwBatteryRejectLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string SendDocNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime SendDate { get; set; }
        public string SenderStore { get; set; }
        public string Remark { get; set; }
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string RowVersionId { get; set; }
    }
    public class vwBatteryRefurbishReceiptLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string SendDocNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime SendDate { get; set; }
        public string SenderStore { get; set; }
        public decimal CarriedCost { get; set; }//added by sanjay
        public decimal BatteryCost { get; set; }
        public decimal RoundAmount { get; set; }
        public string Remark { get; set; }
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string RowVersionId { get; set; }
    }
    public class vwBatteryClaimSettlementLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long BatteryId { get; set; }
        public string BatterySerialNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string DocNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? DocDate { get; set; }
        public string StoreName { get; set; }
        public decimal BatteryRate { get; set; }
        public decimal CGSTPercentage { get; set; }
        public decimal SGSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public string VendorReferenceNo { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal BatteryCGSTAmount { get; set; }
        public decimal BatterySGSTAmount { get; set; }
        public decimal BatteryIGSTAmount { get; set; }
        public decimal RoundAmount { get; set; }
        public decimal BatteryCost { get; set; }
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
    }

    [EdmComplexType]
    public class vwBatteryReceiptLog
    {
        public long? TSLId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? JobSheetId { get; set; }
        public string JobSheetNo { get; set; }
        public long? MechanicId { get; set; }
        public string MechanicName { get; set; }
        public long ReceiptLogId { get; set; }
        public long ReceiptBatteryId { get; set; }
        public string ReceiptBatterySerialNo { get; set; }
        public DateTime? ReceiptOnDate { get; set; }
        public long ReceiptAge { get; set; } = 0;
        public decimal ReceiptAmount { get; set; } = 0;
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public long NextUseId { get; set; }
        
        public string ReceiptRemark { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public long? ReceiptReferenceId { get; set; }
        public string ReceiptRowVersionId { get; set; }
        public string NextUseName { get; set; }
        public long? NextLogId { get; set; }
    }
    [EdmComplexType]
    public class vwBatteryIssueLog
    {
        public long? TSLId { get; set; }
        public long IssueLogId { get; set; }
        public string IssueBatterySerialNo { get; set; }
        public long IssueBatteryId { get; set; }
        public long? IssueReferenceId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public int? GravityLevel { get; set; }
        public bool IsWaterLevelChecked { get; set; }
        public bool IsTerminalCarbonChecked { get; set; }
        public bool IsOld { get; set; }
        public decimal IssueAmount { get; set; }
        public long? JobSheetId { get; set; }
        public string JobSheetNo { get; set; }
        public string IssueRemark { get; set; }
        public long? MechanicId { get; set; }
        public string MechanicName { get; set; }
        public string IssueRowVersionId { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public long? NextLogId { get; set; }
        public long? ReceiptLogId { get; set; }
    }
    
}