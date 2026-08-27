using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("tTyreLifePerf")]
    public class TyreLifePerformanceLog:AuditableEntity,IValidatableObject
    {
        [Column(Order =0),Index("IX_TyrePerformanceLog_TyreId",IsUnique =true,Order =0)]
        public long TyreId { get; set; }
        [ForeignKey("TyreId")]
        public virtual TyreMaster fk_Tyre { get; set; }
        [Column(Order = 1), Index("IX_TyrePerformanceLog_TyreId", IsUnique = true, Order = 1)]
        public int Life { get; set; } = -1;
        public DateTime LifeStartDate { get; set; }
        public DateTime? LifeEndDate { get; set; }
        public long SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Ledger fk_Supplier { get; set; }
        public decimal PurchaseAmount { get; set; } = 0;
        [Column("TyrePreviousMileage")]
        public decimal TyrePreviousMileage { get; set; } = 0;
        [Column("TyreLifeMileage")]
        public decimal TyreLifeMileage { get; set; } = 0;
        public decimal CurrentMileage { get; set; } = 0;
        [Column("JSPreviousMileage")]
        public decimal JSPreviousMileage { get; set; } = 0;
        [Column("JSLifeMileage")]
        public decimal JSLifeMileage { get; set; } = 0;
        [Column("TLPreviousMileage")]
        public decimal TLPreviousMileage { get; set; } = 0;
        [Column("TLLifeMileage")]
        public decimal TLLifeMileage { get; set; } = 0;
        public long? LifeIssueCounts { get; set; } = 0;
        public long? FirstIssueLogId { get; set; }
        [ForeignKey("FirstIssueLogId")]
        public virtual TyreLog fk_FirstIssueLog { get; set; }
        public long? LastReceiptLogId { get; set; }
        [ForeignKey("LastReceiptLogId")]
        public virtual TyreLog fk_LastReceiptLog { get; set; }
        public long? LastTripLogId { get; set; }
        [ForeignKey("LastTripLogId")]
        public virtual TyreLog fk_LastTripLog { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((Life < 0))
            {
                yield return new ValidationResult("Invalid Tyre Life Provided",new []{ "Life" });
            }
            //if (Life > 0 && PreviousMileage <= 0)
            //{
            //    yield return new ValidationResult($"Previous Mileage should be greater then zero when Life is {Life}",new[] { "PreviousMileage" });
            //}
            //if (LifeEndDate.HasValue && LifeMileage <= 0)
            //{
            //   yield return new ValidationResult("When Tyre Life is to end then Life Mileage should be greater than Zero",new []{ "LifeMileage" });
            //}
        }
    }
}