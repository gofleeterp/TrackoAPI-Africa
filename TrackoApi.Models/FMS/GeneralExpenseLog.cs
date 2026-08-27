using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

namespace TrackoApi.Models.FMS
{
    [Table("tGeneralExpenseLog")]
    public class GeneralExpenseLog : AuditableEntity,IValidatableObject
    {
        public GeneralExpenseLog()
        {
            ReferenceNo = VoucherNo;
            ObjectState = ObjectState.Unchanged;            
        }
        [Column("VoucherNo"), StationaryCheck, Required]
        public string VoucherNo { get; set; }
        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long? CNId { get; set; }
        [ForeignKey("CNId")]
        public virtual CNMaster fk_CNMaster { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; }

        [Column("OfficeId")]
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("CreditAccountId"), ForeignKey("fk_CreditAccount")]
        public long CreditAccountId { get; set; }
        public virtual Ledger fk_CreditAccount { get; set; }

        [Column("DebitAccountId"), ForeignKey("fk_DebitAccount")]
        public long DebitAccountId { get; set; }
        public virtual Ledger fk_DebitAccount { get; set; }

        [Column("Amount1")]
        public decimal Amount1 { get; set; } = 0;
        [Column("Amount2")]
        public decimal Amount2 { get; set; } = 0;
        [Column("Amount")]
        public decimal Amount { get; set; } = 0;

        [Column("DriverId"), ForeignKey("fk_Driver")]
        public long? DriverId { get; set; }
        public virtual DriverMaster fk_Driver { get; set; }

        [Column("Remark"), MaxLength(500)]
        public string Remark { get; set; }

        [Column("ReferenceNo"), MaxLength(150)]
        public string ReferenceNo { get; set; }

        [Column("VoucherId"), ForeignKey("fK_Voucher")]
        public long? VoucherId { get; set; }
        public virtual Voucher fK_Voucher { get; set; }
        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherTypeId { get; set; }
        public bool IsBulkEntry { get; set; }
        public long? ViewId { get; set; }
        /// <summary>
        /// Constante Id 1546
        /// </summary>
        public long? ExpenseNatureId { get; set; }
        [ForeignKey("ExpenseNatureId")]
        public virtual GenericMaster fk_ExpenseNature { get; set; }

        public long? TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }
        public long? SettlementId { get; set; }
        [ForeignKey("SettlementId")]
        public virtual VehicleTripSettlement fk_Settlement { get; set; }
        public long? PaidInId { get; set; }
        [ForeignKey("PaidInId")]
        public virtual ConstantValue fk_PaidIn { get; set; }
        [MaxLength(100)]
        public string Ref1 { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }
        public bool GenerateVoucher { get; set; } = true;

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount <= 0)
            {
                yield return new ValidationResult("One of expense is having amount less than Rs.1.\n Which is not allowed.");
            }
        }
    }
}
