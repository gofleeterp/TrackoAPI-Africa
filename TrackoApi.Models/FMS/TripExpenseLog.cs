using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("tTripExpenseLog")]
    public class TripExpenseLog : AuditableEntity
    {
        [Column("TripLogId"), ForeignKey("fk_TripLog"), Required]
        public long TripLogId { get; set; }
        public virtual VehicleMovementLog fk_TripLog { get; set; }

        [Column("ExpenseTypeId"),Required]
        public long ExpenseTypeId { get; set; }
        [ForeignKey("ExpenseTypeId")]
        public virtual ExpenseMaster fk_ExpenseType { get; set; }

        [Column("SettlementId"),ForeignKey("fk_Settlement")]
        public long? SettlementId { get; set; }
        public virtual VehicleTripSettlement fk_Settlement { get; set; }
        [Column("DraftId")]
        public long? DraftId { get; set; }
        public virtual VehicleTripSettlement fk_Draft { get; set; }
        public decimal FuelQty { get; set; } = 0;
        public decimal FuelRate { get; set; } = 0;
        [Column("ClaimAmount")]
        public decimal ClaimAmount { get; set; } = 0;
        [Column("BudgetedQty")]
        public decimal BudgetedQty { get; set; } = 0;
        [Column("SettledAmount")]
        public decimal SettledAmount { get; set; } = 0;
        public decimal ShortFuelQty { get; set; } = 0;
        public decimal ShortFuelAmt { get; set; } = 0;
        [Column("Remarks")]
        [MaxLength(300)]
        public string Remarks { get; set; }
        [ForeignKey("fk_TripAdvanceLog")]
        public long? TripAdvanceLogId { get; set; }
        public virtual TripAdvanceLog fk_TripAdvanceLog{ get; set; }
        public bool IsAuto { get; set; }
        public long? ViewId { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        [MaxLength(150)]
        public string BatchId { get; set; }

        public bool IsBudgeted { get; set; } = false;

    }
}
