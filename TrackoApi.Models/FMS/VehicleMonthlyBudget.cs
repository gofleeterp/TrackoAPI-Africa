using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleMonthlyBudget")]
    public class VehicleMonthlyBudget:AuditableEntity
    {
        [Column("RefDate"), Required, Index("IX_VehicleMonthlyBudget_Unique", IsUnique = true, Order = 0)]
        public DateTime RefDate { get; set; }
        
        [Column("VehicleId"), ForeignKey("fk_Vehicle"), Required, Index("IX_VehicleMonthlyBudget_Unique", IsUnique = true, Order = 1)]
        public long VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("DriverSalary")]
        public decimal DriverSalary { get; set; } = 0;

        [Column("TyreAmount")]
        public decimal TyreExp { get; set; } = 0;

        [Column("RepairAmount")]
        public decimal RepairAmount { get; set; } = 0;

        [Column("DueAmount")]
        public decimal DueAmount { get; set; } = 0;

        [Column("InstallmentAmount")]
        public decimal InstallmentAmount { get; set; } = 0;

        [Column("TripAmount")]
        public decimal TripAmount { get; set; } = 0;

        [Column("OtherExpense")]
        public decimal OtherExpense { get; set; } = 0;

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}
