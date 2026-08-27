using Microsoft.OData.Edm.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;

using TrackoAPI.ViewModels.Global;

namespace TrackoAPI.ViewModels.FMS.Tyres
{
    [EdmComplexType]
    public class vwTyreBillView:IValidatableObject
    {
        public long? ProvisionalAcId { get; set; }
        public string ProvisionalAcName { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
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
        public long? PageId { get; set; }
        [Required]
        public long PrimaryCreditAccountId { get; set; } //2
        public string PrimaryCreditAccountName { get; set; }
        public decimal PrimaryCreditAmount { get; set; } = 0; //2
        [MaxLength(50)]
        public string VendorReferenceNo { get; set; }
        public string Narration { get; set; }
        public long? PrimaryDebitAccountId { get; set; } //1
        public string PrimaryDebitAccountName { get; set; }
        public decimal PrimaryDebitAmount { get; set; } = 0; 
        public long? TyreHSNCodeId { get; set; }
        public string TyreHSNCode { get; set; }

        public long? TubeHSNCodeId { get; set; }
        public string TubeHSNCode { get; set; }
        public long? FlapHSNCodeId { get; set; }
        public string FlapHSNCode { get; set; }

        public long? CGSTLedgerId { get; set; } 
        public decimal CGSTAmount { get; set; } = 0; 

        public long? SGSTLedgerId { get; set; } 
        public decimal SGSTAmount { get; set; } = 0;
       

        public long? IGSTLedgerId { get; set; } 
        public decimal IGSTAmount { get; set; } = 0;

        public long? OtherLedgerId { get; set; } //5
        public long? OtherHSNId { get; set; } //5
        public string OtherLedgerName { get; set; }
        public decimal OtherAmount { get; set; } = 0; //5


        public bool CalVat { get; set; } = false;
        public bool CalOthAmt { get; set; } = false;
        public long? OtherChgRatioId { get; set; }
        public string OtherChgRatio { get; set; }
        public List<vwTyreLog> Tyres { get; set; }
        public List<vwTyreIssueReceipt> IssueReceiptLogs { get; set; }
        public List<vwTyreResaleLog> ResaleLog { get; set; }
        public List<vwTyreClaimLog> ClaimLog { get; set; }
        public List<vwTyreScrapLog> ScrapLog { get; set; }
        public List<vwTyreStoreTransferLog> StoreTransferLog { get; set; }
        public List<vwTyreRejectLog> RejectLog { get; set; }
        public List<vwTyreRemouldReceiptLog> RemouldReceiptLog { get; set; }
        public List<vwTyreClaimSettlementLog> TyreClaimSettlementLog { get; set; }
        public List<vwTyreIssueLog> IssueLogs { get; set; }
        public List<vwTyreReceiptLog> ReceiptLogs { get; set; }
        public string RowVersion_Id { get; set; }
        public string RowVersion_ReceiptId { get; set; }
        public long? ViewId { get; set; }
        public long? TCSAccountId { get; set; }
        public string TCSAccountName { get; set; }
        public decimal TCSAmount { get; set; } = 0;
        public decimal TCSRate { get; set; }
        public decimal PostDiscountAmt { get; set; }
        public decimal RoundOffAmt { get; set; } = 0;       
        public long? PostDiscountAcId { get; set; }
        public long? RoundOffAccId { get; set; }
        public List<JsonDataEntity> JsonData { get; set; }
        public vwTyreBillView()
        {
            Tyres=new List<vwTyreLog>();
            IssueReceiptLogs=new List<vwTyreIssueReceipt>();
            ResaleLog=new List<vwTyreResaleLog>();
            ClaimLog=new List<vwTyreClaimLog>();
            IssueLogs=new List<vwTyreIssueLog>();
            ReceiptLogs=new List<vwTyreReceiptLog>();
            RejectLog=new List<vwTyreRejectLog>();
            JsonData = new List<JsonDataEntity>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            
            if (CalVat && CGSTLedgerId.GetValueOrDefault(0) <= 0 && CGSTAmount > 0)
            {
                yield return new ValidationResult("CGST Account is Required");
            }
            if (CalVat && SGSTLedgerId.GetValueOrDefault(0) <= 0 && SGSTAmount > 0)
            {
                yield return new ValidationResult("SGST Account is Required");
            }
            if (CalVat && IGSTLedgerId.GetValueOrDefault(0) <= 0 && IGSTAmount > 0)
            {
                yield return new ValidationResult("IGST Account is Required");
            }

        }
    }

    [EdmComplexType]
    public class vwTyreLog:IValidatableObject
    {
        public DateTime? LogDate { get; set; }
        public long Id { get; set; }
        public long? TSLId { get; set; }
        public long TyreStatusId { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
        public DateTime? ProductionMonth { get; set; }
        public long? BrandId { get; set; }
        public string BrandName { get; set; }
        public long? RubberTypeId { get; set; }
        public string RubberType { get; set; }
        public long? PurchaseId { get; set; }
        public string PurchaseNo { get; set; }
        public int WarrantyKm { get; set; } = 0;
        public int WarrantyDays { get; set; } = 0;
        public bool CalVat { get; set; } = false;
        public bool IsException { get; set; }
        public decimal Rate { get; set; } = 0;
        public decimal TubeRate { get; set; } = 0;
        public decimal FlapRate { get; set; } = 0;

        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TubeDiscountPercent { get; set; } = 0;
        public decimal TubeDiscountAmount { get; set; } = 0;
        public decimal FlapDiscountPercent { get; set; } = 0;
        public decimal FlapDiscountAmount { get; set; } = 0;

        //public long? TaxServiceTypeId { get; set; }
        //public string TaxServiceType { get; set; }
        public decimal CGSTPercent { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;

        public decimal SGSTPercent { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        public decimal IGSTPercent { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        public decimal TubeCGSTPercent { get; set; } = 0;
        public decimal TubeCGSTAmount { get; set; } = 0;

        public decimal TubeSGSTPercent { get; set; } = 0;
        public decimal TubeSGSTAmount { get; set; } = 0;

        public decimal TubeIGSTPercent { get; set; } = 0;
        public decimal TubeIGSTAmount { get; set; } = 0;

        public decimal FlapCGSTPercent { get; set; } = 0;
        public decimal FlapCGSTAmount { get; set; } = 0;

        public decimal FlapSGSTPercent { get; set; } = 0;
        public decimal FlapSGSTAmount { get; set; } = 0;

        public decimal FlapIGSTPercent { get; set; } = 0;
        public decimal FlapIGSTAmount { get; set; } = 0;

        public decimal TyreTotalAmount { get; set; } = 0;
        public decimal TubeTotalAmount { get; set; } = 0;
        public decimal FlapTotalAmount { get; set; } = 0;


        public decimal SubTotal { get; set; } = 0;
        public decimal TubeSubTotal { get; set; } = 0;
        public decimal FlapSubTotal { get; set; } = 0;

        public decimal OtherAmount { get; set; } = 0;
        public decimal TubeOtherAmount { get; set; } = 0;
        public decimal FlapOtherAmount { get; set; } = 0;

        public decimal RoundUpAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;

        public string Remark { get; set; }
        public long? ReferenceId { get; set; }
        public string ReferenceTyreNo { get; set; }//added by sanjay
        public decimal CarriedCost { get; set; }//added by sanjay
        public long? WheelPositionId { get; set; }
        public string WheelPositionName { get; set; }
        public long? JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public long? MechanicId { get; set; }
        public string Mechanic { get; set; }
        public int NSD { get; set; }
        public int AirPressure { get; set; }
        public long? ReceiptId { get; set; }
        public string ReceiptNo { get; set; }
        public long? ReceiptTyreId { get; set; }
        public string ReceiptTyreNo { get; set; }
        public bool IsStepney { get; set; }
        public long KmReading { get; set; }
        public long KmRun { get; set; }
        public string RowVersionId { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int OpeningKM { get; set; } = 0;
        public int OpeningMonth { get; set; } = 0;
        public long OdoKm { get; set; } = 0;
        public long GpsKm { get; set; } = 0;
        public long TLKm { get; set; } = 0;
        public long JobKm { get; set; } = 0;
        public long? KmSourceId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Rate - DiscountAmount + OtherAmount != SubTotal)
            {
                yield return new ValidationResult($"Tyre SubTotal for Tyre No {TyreNo} has invalid value. It should be {Rate - DiscountAmount + OtherAmount }", new[] { "SubTotal" });
            }
            if (TubeRate - TubeDiscountAmount + TubeOtherAmount != TubeSubTotal)
            {
                yield return new ValidationResult($"SubTotal of Tube for Tyre No {TyreNo} has invalid value. It should be {TubeRate - TubeDiscountAmount + TubeOtherAmount }", new[] { "TubeSubTotal" });
            }
            if (FlapRate - FlapDiscountAmount + FlapOtherAmount != FlapSubTotal)
            {
                yield return new ValidationResult($"SubTotal of Flap for Tyre No {TyreNo} has invalid value. It should be {FlapRate - FlapDiscountAmount + FlapOtherAmount }", new[] { "FlapSubTotal" });
            }
            if (SubTotal + (CalVat ? 0 : CGSTAmount + SGSTAmount + IGSTAmount) != TyreTotalAmount)
            {
                yield return new ValidationResult($"Tyre Item Total of Tyre No {TyreNo} has invalid value. It should be {SubTotal +  CGSTAmount + SGSTAmount + IGSTAmount}", new[] { "TyreTotalAmount" });
            }
            if (TubeSubTotal + (CalVat ? 0 : TubeCGSTAmount + TubeSGSTAmount + TubeIGSTAmount) != TubeTotalAmount)
            {
                yield return new ValidationResult($"Tube Item Total of Tyre No {TyreNo} has invalid value. It should be {TubeSubTotal + TubeCGSTAmount + TubeSGSTAmount + TubeIGSTAmount}", new[] { "TubeSubTotal" });
            }
            if (FlapSubTotal + (CalVat ? 0 : FlapCGSTAmount + FlapSGSTAmount + FlapIGSTAmount) != FlapTotalAmount)
            {
                yield return new ValidationResult($"Flap Item Total of Tyre No {TyreNo} has invalid value. It should be {FlapSubTotal + FlapCGSTAmount + FlapSGSTAmount + FlapIGSTAmount}", new[] { "FlapSubTotal" });
            }

            if (TyreTotalAmount + TubeTotalAmount + FlapTotalAmount + RoundUpAmount != NetAmount)
            {
                yield return new ValidationResult($"NetAmount for Tyre No {TyreNo} has invalid value.");
            }
            if (string.IsNullOrWhiteSpace(TyreNo))
            {
                yield return new ValidationResult("Tyre No is Required", new[] { "TyreNo" });
            }
        }
    }
    [EdmComplexType]
    public class vwTyreIssueReceipt
    {
        public long? TSLId { get; set; }
        public long IssueLogId { get; set; }
        public string IssueTyreNo { get; set; }
        public long IssueTyreId { get; set; }
        public long? IssueReferenceId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long IssueOnKM { get; set; }
        public long? WheelPositionId { get; set; }
        public string WheelPositionName { get; set; }
        public decimal IssuePSI { get; set; }
        public bool IsOld { get; set; }
        public bool IsStepney { get; set; }
        public bool IsException { get; set; }
        public decimal IssueAmount { get; set; }
        public long? JobSheetId { get; set; }
        public string JobSheetNo { get; set; }
        public string IssueRemark { get; set; }
        public long? MechanicId { get; set; }
        public string MechanicName { get; set; }
        public string IssueRowVersionId { get; set; }
        public long ReceiptLogId { get; set; }
        public long ReceiptTyreId { get; set; }
        public string ReceiptTyreNo { get; set; }
        public DateTime? ReceiptOnDate { get; set; }
        public long ReceiptOnKm { get; set; } = 0;
        public long ReceiptOutKm { get; set; } = 0;
        public long ReceiptKmRun { get; set; } = 0;
        public long ReceiptMonth { get; set; } = 0;
        public decimal ReceiptAmount { get; set; } = 0;
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public long NextUseId { get; set; }
        public decimal ReceiptTreadWear { get; set; } = 0;
        public decimal IssueTreadWear { get; set; } = 0;
        public string ReceiptRemark { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public long? ReceiptReferenceId { get; set; }
        public string ReceiptRowVersionId { get; set; }
        public string NextUseName { get; set; }
        public long? NextLogId { get; set; }
    }
    [EdmComplexType]
    public class vwTyreReceiptLog
    {
        public long? TSLId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? JobSheetId { get; set; }
        public string JobSheetNo { get; set; }
        public long? MechanicId { get; set; }
        public string MechanicName { get; set; }
        public long ReceiptLogId { get; set; }
        public long ReceiptTyreId { get; set; }
        public string ReceiptTyreNo { get; set; }
        public DateTime? ReceiptOnDate { get; set; }
        public long ReceiptOnKm { get; set; } = 0;
        public long ReceiptOutKm { get; set; } = 0;
        public long ReceiptKmRun { get; set; } = 0;
        public long ReceiptMonth { get; set; } = 0;
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

        public long? WheelPositionId { get; set; }
        public string WheelPositionName { get; set; }
        public decimal AirPressure { get; set; }
        public decimal NSD1 { get; set; } = 0;
        public decimal NSD2 { get; set; } = 0;
        public decimal NSD3 { get; set; } = 0;
        public decimal NSD4 { get; set; } = 0;
        public long OdoKm { get; set; } = 0;
        public long GpsKm { get; set; } = 0;
        public long TLKm { get; set; } = 0;
        public long JobKm { get; set; } = 0;
        public long? KmSourceId { get; set; }
        public bool IsException { get; set; }
    }
    [EdmComplexType]
    public class vwTyreIssueLog
    {
        public long? TSLId { get; set; }
        public long IssueLogId { get; set; }
        public string IssueTyreNo { get; set; }
        public long IssueTyreId { get; set; }
        public long? IssueReferenceId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long IssueOnKM { get; set; }
        public long? WheelPositionId { get; set; }
        public string WheelPositionName { get; set; }
        public decimal IssuePSI { get; set; }
        public decimal NSD1 { get; set; } = 0;
        public decimal NSD2 { get; set; } = 0;
        public decimal NSD3 { get; set; } = 0;
        public decimal NSD4 { get; set; } = 0;
        public bool IsOld { get; set; }
        public bool IsStepney { get; set; }
        public bool IsException { get; set; }
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
    [EdmComplexType]
    public class vwTyreChassisBill
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

        public List<vwTyreLog> TyreLogs { get; set; }
        public long? PageId { get; set; }
        public long? ViewId { get; set; }
        public bool IsException { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
    }

    [EdmComplexType]
    public class vwTyreResaleLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
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
        public bool IsException { get; set; }
    }
    [EdmComplexType]
    public class vwTyreClaimLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
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
        public bool IsException { get; set; }
    }
    [EdmComplexType]
    public class vwTyreScrapLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string ReceiptNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReceivedFrom { get; set; }
        public decimal TyreCost { get; set; }
        public decimal CGSTPercent { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;

        public decimal SGSTPercent { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        public decimal IGSTPercent { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;
        public decimal TyreTotalAmount { get; set; } = 0;
        public decimal RoundUpAmount { get; set; } = 0;
        
        public decimal NetAmount { get; set; } = 0;
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
        public bool IsException { get; set; }
    }
    public class vwTyreStoreTransferLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string ReceiptNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReceivedFrom { get; set; }
        public decimal TyreCost { get; set; }
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
        public bool IsException { get; set; }
    }
    public class vwTyreRejectLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
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
        public bool IsException { get; set; }
    }
    public class vwTyreRemouldReceiptLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public long? RubberTypeId { get; set; }
        public string RubberType { get; set; }
        public string SendDocNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime SendDate { get; set; }
        public string SenderStore { get; set; }
        public decimal CarriedCost { get; set; }//added by sanjay
        public decimal Amount { get; set; } = 0;
        public long? ServiceTaxTypeId { get; set; }
        public string ServiceTaxType { get; set; }
        public decimal CGSTPercentage { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTPercentage { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTPercentage { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;
        public decimal RoundUpAmount { get; set; } = 0;
        public decimal TyreCost { get; set; }
        public string Remark { get; set; }
        public long? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string RowVersionId { get; set; }
        public bool IsException { get; set; }
    }
    [EdmComplexType]
    public class vwTyreClaimSettlementLog
    {
        public long? TSLId { get; set; }
        public long Id { get; set; }
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string DocNo { get; set; }
        public long ReferenceId { get; set; }
        public DateTime? DocDate { get; set; }
        public string StoreName { get; set; }
        public decimal TyreRate { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTPercentage { get; set; }
        public decimal SGSTPercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public string VendorReferenceNo { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal RoundUpAmount { get; set; } = 0;
        public decimal TyreCost { get; set; }
        public string Remark { get; set; }
        public string RowVersionId { get; set; }
        public bool IsException { get; set; }
    }
}