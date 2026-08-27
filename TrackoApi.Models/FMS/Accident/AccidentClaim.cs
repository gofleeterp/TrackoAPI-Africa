using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleAccident")]
    public class VehicleAccidentClaim : AuditableEntity
    {
        #region Accident Details

        [Column("OfficeId"), ForeignKey("fk_Office")]
        public long OfficeId { get; set; }

        public virtual OfficeMaster fk_Office { get; set; }
        [MaxLength(200), StationaryCheck]
        public string DocumentNo { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? AccidentDate { get; set; }

        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long VehicleId { get; set; }

        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("AccidentPlaceId"), ForeignKey("fk_AccidentPlace")]
        public long? AccidentPlaceId { get; set; }

        public virtual CityMaster fk_AccidentPlace { get; set; }


        [Column("DriverId"), ForeignKey("fk_Driver")]
        public long? DriverId { get; set; }

        public virtual DriverMaster fk_Driver { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }

        #endregion

        #region Insurance Details
        [MaxLength(300)]
        public string PolicyNo { get; set; }

        [Column("InsCompanyId"), ForeignKey("fk_InsuranceCompany")]
        public long? InsCompanyId { get; set; }

        public virtual Ledger fk_InsuranceCompany { get; set; }

        public decimal InsuranceAmount { get; set; }
        public bool Comprehensive { get; set; }
        public decimal InsuranceClaimAmount { get; set; }

        [Column("DevelopmentOfficer")]
        [MaxLength(100)]
        public string DevelopmentOfficerName { get; set; }
        [MaxLength(100)]
        public string AgentName { get; set; }
        [MaxLength(25)]
        public string AgentContactNo { get; set; }

        #endregion

        #region FIR Details
        [MaxLength(200)]
        public string FIRNo { get; set; }
        public DateTime? FIRDate { get; set; }
        [MaxLength(300)]
        public string PoliceStation { get; set; }
        [MaxLength(100)]
        public string PoliceOfficer { get; set; }
        [MaxLength(25)]
        public string PoliceContactNo { get; set; }

        #endregion

        #region Inspection Details

        public DateTime? InspectionDate { get; set; }
        [MaxLength(200)]
        public string InspectionBy { get; set; }
        [MaxLength(500)]
        public string InspectionRemark { get; set; }

        #endregion

        #region Survey Details

        [Column("SSurveyorName"), MaxLength(100)]
        public string SSurveyorName { get; set; }

        public DateTime? SSurveyDate { get; set; }

        [Column("SSurveyorContactNo"), MaxLength(50)]
        public string SSurveyorContactNo { get; set; }


        [Column("SSurveyRemark"), MaxLength(1000)]
        public string SSurveyRemark { get; set; }


        [Column("FSurveyorName"), MaxLength(100)]
        public string FSurveyorName { get; set; }

        public DateTime? FSurveyDate { get; set; }

        [Column("FSurveyorContactNo"), MaxLength(50)]
        public string FSurveyorContactNo { get; set; }

        [Column("FSurveyRemark"), MaxLength(1000)]
        public string FSurveyRemark { get; set; }

        [Column("RepairingLocation"), MaxLength(100)]
        public string RepairingLocation { get; set; }

        [Column("RepairingAdd"), MaxLength(1000)]
        public string RepairingAdd { get; set; }

        [Column("RepairingRemark"), MaxLength(1000)]
        public string RepairingRemark { get; set; }

        #endregion

        #region Settlement Details
        public long CashBankAcId { get; set; }
        public long SettlemenAcId { get; set; }
        public decimal SettlementAmount { get; set; } = 0;
        [MaxLength(100)]
        public string BankName { get; set; }
        [MaxLength(20)]
        public string ChequeNo { get; set; }
        [MaxLength(20)]
        public string ChequeDate { get; set; }
        public decimal ChequeAmount { get; set; } = 0;
        [MaxLength(200)]
        public string Description { get; set; }
        #endregion

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
    }
}
