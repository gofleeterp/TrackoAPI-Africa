using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.ViewModels.FMS;

namespace TrackoApi.Models.BMS
{
    [Table("tChallanMaster")]
    public class ChallanMaster : AuditableEntity,IValidatableObject
    {
        public ChallanMaster()
        {
            ObjectState=ObjectState.Unchanged;
        }
        [Column("OfficeID"), Required,ForeignKey("fk_Office")]
        public long OfficeID { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("ChallanNo"), StationaryCheck, Required, MaxLength(50), MinLength(3),Index("XI_ChallanMaster_ChallanNo",IsUnique = true)]
        public string ChallanNo { get; set; }

        [Column("ChallanDate"), Required]
        public DateTime ChallanDate { get; set; }

        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("HireVehicleId"), ForeignKey("fk_HireVehicle")]
        public long? HireVehicleId { get; set; }
        public virtual HireVehicle fk_HireVehicle { get; set; }

        [Column("RouteId"), ForeignKey("fk_Route")]
        public long? RouteId { get; set; }
        public virtual RouteMaster fk_Route { get; set; }

        [Column("Quantity")]
        public decimal Quantity { get; set; }

        [Column("ActualWeight")]
        public decimal Weight { get; set; }

        [Column("Remarks"), MaxLength(500)]
        public string Remarks { get; set; }

        [Column("ArrivalDate")]
        public DateTime? ArrivalDate { get; set; }
        public DateTime? UnloadDate { get; set; }
        public string ArrivalRemark { get; set; }

        [Column("ChallanTypeId"), ForeignKey("fk_ChallanType")]
        public long? ChallanTypeId { get; set; }
        public virtual ConstantValue fk_ChallanType { get; set; }

        //[Column("ChallanModeId"), ForeignKey("fk_ChallanMode")]
        //public long ChallanModeId { get; set; }
        //public virtual ConstantValue fk_ChallanMode { get; set; }
        [Column("DeliveredTo"), MaxLength(100)]
        public string DeliveredTo { get; set; }

        [Column("AdditionalCharges")]
        public decimal AdditionalCharges { get; set; }
        [Column("PartyName"), MaxLength(100)]
        public string PartyName { get; set; }
        [Column("TriplogId"), ForeignKey("fk_Triplog")]
        public long? TriplogId { get; set; }

        public virtual VehicleMovementLog fk_Triplog { get; set; }
        [Column("DriverName"), MaxLength(100)]
        public string DriverName { get; set; }
        [Column("DriverId"), ForeignKey("fk_Driver")]
        public long? DriverId { get; set; }
        public virtual DriverMaster fk_Driver { get; set; }
        [Column("MobileNo"), MaxLength(100)]
        public string MobileNo { get; set; }
        [Column("ExpectedDate")]
        public DateTime? ExpectedDate { get; set; }


        [Column("eWayBillNo"), MaxLength(200)]
        public string eWayBillNo { get; set; }

        public DateTime? eWayBillExpiryDate { get; set; }

        [Column("ConsigneeId"), ForeignKey("fk_Consignee")]
        public long? ConsigneeId { get; set; }
        public virtual Ledger fk_Consignee { get; set; }

        [Column("ConsignorId"), ForeignKey("fk_Consignor")]
        public long? ConsignorId { get; set; }
        public virtual Ledger fk_Consignor { get; set; }

        [Column("BillingPartyId"), ForeignKey("fk_BillingParty")]
        public long? BillingPartyId { get; set; }
        public virtual Ledger fk_BillingParty { get; set; }
        
        /// <summary>
        /// Every Challan gets created from some form directly or indirectly, 
        /// So We Showl maintain the Id of the Form from which the Challan was Triggred.
        /// </summary>
        public long ViewId { get; set; }
        public virtual List<CnChallan> CNChallans { get; set; }
        public virtual List<vwChallanCN> ChallanCNView { get; set; }
        public string CnChallanJson { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.UnloadDate == null && this.ArrivalDate != null)
            {
                this.UnloadDate = this.ArrivalDate;
            }
            else if (this.UnloadDate != null && this.ArrivalDate == null)
            {
                this.ArrivalDate = this.UnloadDate;
            }
            if (string.IsNullOrWhiteSpace(ChallanNo))
            {
                yield return new ValidationResult("Challan Number is required");
            }
        }
    }
}