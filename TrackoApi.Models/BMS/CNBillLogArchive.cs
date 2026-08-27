using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tCNBillLogArchive")]
    public class CNBillLogArchive : AuditableEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [Column("BillId"), Index("IDX_CNBillLogArchive_UniqueKey", IsUnique = true,Order = 0)]
        public long BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual CNBill fk_Bill { get; set; }
        [Column("TripLogId"), Index("IDX_CNBillLogArchive_UniqueKey", IsUnique = true, Order = 2)]
        public long? TripLogId { get; set; }
        public string TripLogNo { get; set; }
        [Column("CNId"), Index("IDX_CNBillLogArchive_UniqueKey", IsUnique = true, Order = 1)]
        public long? CNId { get; set; }
        
        public string CNNo { get; set; }

        public long? SalesLogId { get; set; }
        /// <summary>
        /// ContantId 
        /// </summary>
        //[Column("ParticularId"), Index("IDX_CNBillLogArchive_UniqueKey", IsUnique = true, Order = 3)]
        public long? ParticularId { get; set; }
        [ForeignKey("ParticularId")]
        public virtual GenericMaster fk_Particular { get; set; }
        public long? HSNCodeId { get; set; }
        [ForeignKey("HSNCodeId")]
        public virtual TaxServiceType fk_HSNCode { get; set; }
        [Column("BillingPartyAcId")]
        public long BillingPartyAccountId { get; set; }
        [ForeignKey("BillingPartyAccountId")]
        public virtual Ledger fk_BillingPartyAc { get; set; }
        /// <summary>
        /// Gets or sets the rate1.
        /// <remarks>Only Used in case of supplementory Bill in it is of RateDifference bill
        /// and will contain old rate of CN 
        /// </remarks>
        /// </summary>
        /// <value>The rate1.</value>
        [Precision(28, 10)]
        public decimal OldRate { get; set; }
        /// <summary>
        /// Gets or sets the rate.
        /// New Rate Of CN and used in case of Supplementary Bill
        /// </summary>
        /// <value>The rate.</value>
        [Precision(28, 10)]
        public decimal NewRate { get; set; }
        /// <summary>
        /// Gets or sets the freight calculate criteria.
        /// <remarks>Only used in Supplementary Bill and used to calculate CNFreight</remarks>
        /// </summary>
        /// <value>The freight calculate criteria.</value>
        [Precision(28, 10)]
        public decimal FreightCalcCriteria { get; set; }
        /// <summary>
        /// Gets or sets the cn freight.
        /// <remarks>Rate*FreightCalcCriteria only in case of Supplementary Bill
        /// In case of Normal Freight this would be CN Final Total Freight after calculating all less and additions amounts
        /// CNSubTotalII Field of CnMaster
        /// </remarks>
        /// </summary>
        /// <value>The cn freight.</value>
        [Precision(28, 10)]
        public decimal CNFreight { get; set; }//1000
        /// <summary>
        /// Gets or sets the discount rate.
        /// <remarks>Incase discount to be recorded in bill</remarks>
        /// </summary>
        /// <value>The discount rate.</value>
        [Precision(28, 10)]
        public decimal DiscountRate { get; set; }
        /// <summary>
        /// Gets or sets the discount amount.
        /// <remarks>Incase discount to be recorded in bill</remarks>
        /// </summary>
        /// <value>The discount amount.</value>
        public decimal DiscountAmount { get; set; }//200

        /// <summary>
        /// Gets or sets the subtotal 1.
        /// </summary>
        /// <value>The subtotal 1.</value>
        public decimal Subtotal1 { get; set; }//800
        #region Only Used in Supplementary Bill
        public decimal AOther1Amount { get; set; }
        public decimal AOther2Amount { get; set; }
        public decimal AOther3Amount { get; set; }
        public decimal AOther4Amount { get; set; }
        public decimal AOther5Amount { get; set; }
        public decimal AOther6Amount { get; set; }

        public decimal LOther1Amount { get; set; }
        public decimal LOther2Amount { get; set; }
        public decimal LOther3Amount { get; set; }
        public decimal LOther4Amount { get; set; }
        #endregion        

        /// <summary>
        /// Gets or sets the subtotal2.
        /// <remarks>In case of Supplementary Bill it would include all above charges included in supplementary Bill region
        /// In case of Normal Bill it would SubTotal1
        /// </remarks>
        /// </summary>
        /// <value>The subtotal2.</value>
        public decimal Subtotal2 { get; set; }//800
        [Precision(28, 7)]
        public decimal NonTaxableAmount { get; set; }//200
        /// <summary>
        /// Gets or sets the ist amount.
        /// Cn Service Tax Amount
        /// </summary>
        /// <value>The ist amount.</value>
        public decimal ISTAmount { get; set; }//1000


        public long? IGSTACId { get; set; }
        [ForeignKey("IGSTACId")]
        public virtual Ledger fk_IGSTAC { get; set; }

        public decimal IGSTRate { get; set; } = 0;

        public decimal IGSTAmount { get; set; } = 0;//1000


        public long? CGSTACId { get; set; }
        [ForeignKey("CGSTACId")]
        public virtual Ledger fk_CGSTAC { get; set; }
        public decimal CGSTRate { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;

        public long? SGSTACId { get; set; }
        [ForeignKey("SGSTACId")]
        public virtual Ledger fk_SGSTAC { get; set; }
        public decimal SGSTRate { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the sub total3.
        /// <remarks>SubTotal2+</remarks>
        /// </summary>
        /// <value>The sub total3.</value>
        public decimal SubTotal3 { get; set; }
        [Precision(28, 10)]
        public decimal RoundOff { get; set; }
        /// <summary>
        /// Gets or sets the total bill amount.
        /// <remarks>
        /// Subtotal3-RoundOff
        /// </remarks>
        /// </summary>
        /// <value>The total bill amount.</value>
        [Precision(28, 10)]
        public decimal TotalBillAmount { get; set; }

        public virtual List<CNBillPaymentLog> PaymentLogs { get; set; }
        [Precision(28, 10)]
        public decimal BalanceAmount { get; set; } = 0;        
        public string UserRemark { get; set; }

    }
}
