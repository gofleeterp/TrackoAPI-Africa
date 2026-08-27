using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Models.BMS
{
    [Table("tSalesLog")]
    public class SalesLog : AuditableEntity
    {
        public long? CNId { get; set; }
        [ForeignKey("CNId")]
        public virtual CNMaster fk_CN { get; set; }

        public long? TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }

        public long? ChallanCNId { get; set; }
        [ForeignKey("ChallanCNId")]
        public virtual CnChallan fk_ChallanCN { get; set; }

        [Column("DocDate")]
        public DateTime? DocDate { get; set; }

        [MaxLength(100), Required]
        public string DocNo { get; set; }

        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }

        public long? RateChartId { get; set; }
        [ForeignKey("RateChartId")]
        public virtual CNRateContract fk_RateChart { get; set; }

        public long? RateId { get; set; }
        [ForeignKey("RateId")]
        public virtual CNRateContractLog fk_Rate { get; set; }

        public long? BillingPartyId { get; set; }
        [ForeignKey("BillingPartyId")]
        public virtual Ledger fk_BillingParty { get; set; }

        public long? BillingOfficeId { get; set; }
        [ForeignKey("BillOffice")]
        public virtual OfficeMaster fk_BillOffice { get; set; }

        public long? SalesOfficeId { get; set; }
        [ForeignKey("SalesOfficeId")]
        public virtual OfficeMaster fk_SalesOffice { get; set; }

        public long? SalesAccountId { get; set; }
        [ForeignKey("SalesAccountId")]
        public virtual Ledger fk_SalesAccount { get; set; }

        public long? UnbilledSalesAcId { get; set; }
        [ForeignKey("UnbilledSalesAcId")]
        public virtual Ledger fk_UnbilledSalesAccount { get; set; }
        //public long? BillId { get; set; }
        //[ForeignKey("BillId")]
        //public virtual CNBill fk_Bill { get; set; }

        [Precision(28, 10)]
        [Column("ChargeWeight")]
        public decimal ChargeWeight { get; set; }

        public long? ChargeWeightUnitId { get; set; }
        [ForeignKey("ChargeWeightUnitId")]
        public virtual UnitMaster fk_ChargeWeightUnit { get; set; }

        [Column("ChargeQty")]
        public decimal ChargeQty { get; set; } = 0;

        public long? ChargeQtyUnitId { get; set; }
        [ForeignKey("ChargeQtyUnitId")]
        public virtual UnitMaster fk_ChargeQtyUnit { get; set; }


        [Column("ActualQty")]
        public decimal ActualQty { get; set; }

        public long? ActualQtyUnitId { get; set; }
        [ForeignKey("ActualQtyUnitId")]
        public virtual UnitMaster fk_ActualQtyUnit { get; set; }

        [Precision(28, 10)]
        [Column("ActualWeight")]
        public decimal ActualWeight { get; set; }

        public long? ActualWeightUnitId { get; set; }
        [ForeignKey("ActualWeightUnitId")]
        public virtual UnitMaster fk_ActualWeightUnit { get; set; }
        #region SubTotal
        [Column("Rate")]
        [Precision(28, 10)]
        public decimal Rate { get; set; }

        [Precision(28, 10)]
        [Column("BasicFreight")]
        public decimal BasicFreight { get; set; }

        [Column("DiscPercent")]
        public decimal DiscPercent { get; set; }

        [Column("Discount")]
        [Precision(28, 10)]
        public decimal Discount { get; set; }

        [Precision(28, 10)]
        [Column("SubTotal")]
        public decimal SubTotal { get; set; }
        #endregion
        #region GrossFreight
        public decimal AChargeI { get; set; } = 0;

        public decimal AChargeII { get; set; } = 0;

        public decimal AChargeIII { get; set; } = 0;

        public decimal AChargeIV { get; set; } = 0;

        public decimal AChargeV { get; set; } = 0;

        public decimal AChargeVI { get; set; } = 0;

        public decimal AChargeVII { get; set; } = 0;

        public decimal AChargeVIII { get; set; } = 0;

        public decimal AChargeIX { get; set; } = 0;

        public decimal AChargeX { get; set; } = 0;

        public decimal LChargeI { get; set; } = 0;

        public decimal LChargeII { get; set; } = 0;

        public decimal LChargeIII { get; set; } = 0;

        public decimal LChargeIV { get; set; } = 0;

        public decimal LChargeV { get; set; } = 0;

        public decimal LChargeVI { get; set; } = 0;

        public decimal LChargeVII { get; set; } = 0;

        public decimal LChargeVIII { get; set; } = 0;

        public decimal LChargeIX { get; set; } = 0;

        public decimal LChargeX { get; set; } = 0;

        public decimal LDetentionRate { get; set; } = 0;

        public decimal LDetentionDays { get; set; } = 0;

        public decimal ULDetentionRate { get; set; } = 0;

        public decimal ULDetentionDays { get; set; } = 0;

        public decimal LDPenaltyRate { get; set; } = 0;

        public decimal LDPenaltyDays { get; set; } = 0;

        [Precision(28, 10)]
        public decimal GrossFreight { get; set; } = 0;
        #endregion
        #region NetFreight

        [Precision(28, 10)]
        public decimal IGSTAmount { get; set; } = 0;

        public decimal IGSTRate { get; set; } = 0;

        [Precision(28, 10)]
        public decimal CGSTAmount { get; set; } = 0;

        public decimal CGSTRate { get; set; } = 0;

        [Precision(28, 10)]
        public decimal SGSTAmount { get; set; } = 0;

        public decimal SGSTRate { get; set; } = 0;

        [Precision(28, 10)]
        public decimal NetFreight { get; set; } = 0;

        #endregion
        public long? IGSTACId { get; set; }
        [ForeignKey("IGSTACId")]
        public virtual Ledger fk_IGSTAC { get; set; }

        public long? CGSTACId { get; set; }
        [ForeignKey("CGSTACId")]
        public virtual Ledger fk_CGSTAC { get; set; }

        public long? SGSTACId { get; set; }
        [ForeignKey("SGSTACId")]
        public virtual Ledger fk_SGSTAC { get; set; }

        public long? GSTPaidById { get; set; }
        [ForeignKey("GSTPaidById")]
        public virtual ConstantValue fk_GSTPaidBy { get; set; }

        public long? GSTServiceTypeId { get; set; }
        [ForeignKey("GSTServiceTypeId")]
        public virtual TaxServiceType fk_GSTServiceType { get; set; }

        //public long? DeliveryTypeId { get; set; }
        //[ForeignKey("DeliveryTypeId")]
        //public virtual ConstantValue fk_DeliveryType { get; set; }

        //public DateTime? DeliveryDate { get; set; }
        /// <summary>
        /// Indicates whether NetFreight Includes Taxes
        /// </summary>
        public bool IsTaxApplicable { get; set; } = true;
        public virtual List<CNBillLog> BillLogs { get; set; }
        /// <summary>
        /// Sales Voucher VDR
        /// </summary>
        public long? VDRId { get; set; }
        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }
        /// <summary>
        /// Sales Voucher
        /// </summary>
        public long? SalesVoucherId { get; set; }
        [ForeignKey("SalesVoucherId")]
        public virtual Voucher fk_SalesVoucher { get; set; }
        [MaxLength(200)]
        public string BatchId { get; set; }

        public void PrepareSalesVoucher(ref Voucher v)
        {
            if(v ==null)
            {
                v = new Voucher();
            }
            //var fs = Helper.GetFinanceStatus();
            v.VoucherTypeId = 89;
            v.ObjectState = v.Id==0?ObjectState.Added:ObjectState.Modified;
            v.Account1Id = this.SalesAccountId;
            v.Account2Id = this.UnbilledSalesAcId;
            v.Amount1 = -this.GrossFreight;
            v.Amount2 = this.GrossFreight;
            v.VoucherAmount = this.GrossFreight;
            v.VoucherDate = this.DocDate??DateTime.Today;
            v.VoucherDateTime = this.DocDate??DateTime.Now;
            v.AccountingRemark = $"Sales Booked against{(this.CNId>0?" CN No :":" Trip No : ")}{this.DocNo} dt: {this.DocDate??DateTime.Today:D}";
            v.VoucherNo = $"S{(this.CNId > 0 ? "CN" : "TP")}-{this.DocNo}";
            v.VDCount = 2;
            v.ReferenceTransactionId = this.Id;
            v.OfficeId = this.SalesOfficeId??BillingOfficeId??0;
            //switch (fs)
            //{
            //    case FinanceStatus.NA:
            //        v.IsAccountsVisiblity = false;
            //        v.IsAccepted = false;
            //        v.IsAudited = false;
            //        break;
            //    case FinanceStatus.DirectImport:
                    v.IsAccountsVisiblity = true;
                    v.IsAccepted = true;
                    v.IsAudited = false;
            //        break;
            //    case FinanceStatus.ApprovalRequired:
            //        v.IsAccountsVisiblity = true;
            //        v.IsAccepted = false;
            //        v.IsAudited = false;
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
            var vd1 =v.VoucherDetails.FirstOrDefault(x=>x.OrderId==1)?? new VoucherDetail();
            vd1.OfficeId = v.OfficeId;
            vd1.ObjectState = vd1.Id == 0 ? ObjectState.Added : ObjectState.Modified;
            vd1.AccountId = v.Account1Id ?? 0;
            vd1.Amount = v.Amount1;
            vd1.OrderId = 1;
            vd1.Voucher = v;
            vd1.VoucherId = v.Id;
            vd1.BatchId = v.BatchId;
            if (vd1.Id == 0) v.VoucherDetails.Add(vd1);


            var vd2 = v.VoucherDetails.FirstOrDefault(x => x.OrderId == 2) ?? new VoucherDetail();
            vd2.OfficeId = v.OfficeId;
            vd2.ObjectState = vd2.Id == 0 ? ObjectState.Added : ObjectState.Modified;
            vd2.AccountId = v.Account2Id ?? 0;
            vd2.Amount = v.Amount2;
            vd2.OrderId = 2;
            vd2.Voucher = v;
            vd2.VoucherId = v.Id;
            vd2.BatchId = v.BatchId;
            if (vd2.Id == 0) v.VoucherDetails.Add(vd2);

            var vdr1=vd2.VoucherDetailReferences.FirstOrDefault()?? new VoucherDetailReference();
            vdr1.AccountId = vd2.AccountId;
            vdr1.Amount = vd2.Amount;
            vdr1.DueDate = this.DocDate??DateTime.Today;
            vdr1.ObjectState = vdr1.Id == 0 ? ObjectState.Added : ObjectState.Modified;
            vdr1.ReferenceNo = v.VoucherNo;
            vdr1.VDRTypeId = 1013;
            vdr1.VoucherDetailId = vd2.Id;
            vdr1.fk_VoucherDetail = vd2;
            vdr1.BatchId = v.BatchId;
            if (vdr1.Id==0)
            {
                vd2.VoucherDetailReferences = new List<VoucherDetailReference>()
                {
                    vdr1
                };
            }

            this.fk_VDR = vdr1;
            this.VDRId = vdr1.Id;
            this.SalesVoucherId = v.Id;
            this.fk_SalesVoucher = v;
        }
    }
}