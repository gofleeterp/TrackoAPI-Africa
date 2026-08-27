using System;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mTripExpenseBudget")]
    public class TripExpenseBudget : AuditableEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndtDate { get; set; }
        public long ExpenseTypeId { get; set; }
        [ForeignKey("ExpenseTypeId")]
        public virtual ExpenseMaster fk_ExpenseType { get; set; }

        public long? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]
        public virtual GenericMaster fk_VehicleType { get; set; }

        public long? ClassId { get; set; }
        [ForeignKey("ClassId")]
        public virtual ObjectClass fk_Class { get; set; }

        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }

        public long? ScriptId { get; set; }
        [ForeignKey("ScriptId")]
        public virtual ApiWorkFlowScript fk_Script { get; set; }
        public decimal ExpenseValue { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }

        [Column("TripNatureId"), ForeignKey("fk_TripNature")]
        public long? TripNatureId { get; set; } //Loaded//Empty
        /// <summary>
        /// Gets or sets the FK_ trip nature.
        /// Constant TypeId 72
        /// </summary>
        /// <value>The FK_ trip nature.</value>
        public virtual ConstantValue fk_TripNature { get; set; }
        public long? ViewId { get; set; }

        public long? TripModeId { get; set; }
        [ForeignKey("TripModeId")]
        public virtual GenericMaster fk_TripMode { get; set; }
    }
}
