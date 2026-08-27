using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.FMS.Driver;

namespace TrackoApi.Models.FMS
{
    [Table("mDriverMaster")]
    public class DriverMaster : AuditableEntity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [ForeignKey("Id")]
        public virtual Ledger fk_Ledger { get; set; }
        [Column("OfficeID"),ForeignKey("fk_Office"),Required]
        public long OfficeId { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("RegistrationDate"),Required]
        public DateTime RegistrationDate { get; set; }
        
        [Column("DriverCode"), StationaryCheck, Index("IDX_mDriverMaster_DriverCode", IsUnique = true), Required, MaxLength(100)]
        public string DriverCode { get; set; }

        [Column("DriverName"), Index("IDX_mDriverMaster_DriverName", IsUnique = true), Required, MaxLength(150)]
        public string DriverName { get; set; }
        [MaxLength(400)]
        public string NameOnLicence { get; set; }

        [Column("DateOfBirth"),Required]
        public DateTime DateOfBirth { get; set; }

        [Column("Age")]
        public long? Age { get; set; }

        [Column("ReligionId"), ForeignKey("fk_DriverReligion")]
        public long? ReligionId { get; set; }
        public virtual ConstantValue fk_DriverReligion { get; set; }

        [Column("MaritalStatus"),Required]
        public bool? MaritalStatus { get; set; }

        [Column("BloodGroupId"), ForeignKey("fk_BloodGroup")]
        public long? BloodGroupId { get; set; }
        public virtual ConstantValue fk_BloodGroup { get; set; }

        [Column("NHIMANo"), MaxLength(50)]
        public string NHIMANo { get; set; }

        [Column("TPINNo"), MaxLength(50)]
        public string TPINNo { get; set; }

        [Column("NRCNo"), MaxLength(50)]
        public string NRCNo { get; set; }

        [Column("NAPSAAcNo"), MaxLength(50)]
        public string NAPSAAcNo { get; set; }


        [Column("PassportNo"), MaxLength(50)]
        public string PassportNo { get; set; }

        [Column("PassportIssueDate")]
        public DateTime? PassportIssueDate { get; set; }

        [Column("PassportExpiryDate")]
        public DateTime? PassportExpiryDate { get; set; }


        [Column("PFNo"), MaxLength(50)]
        public string PFNo { get; set; }
        [Column("UAN"), MaxLength(50)]
        public string UAN { get; set; }
        [Column("ESICNo"), MaxLength(50)]
        public string ESICNo { get; set; }

        [Column("LicenceNo"), MaxLength(50)]
        public string LicenceNo { get; set; }

        [Column("LicenceDate")]
        public DateTime? LicenceDate { get; set; }

        [Column("IssuingPlaceId"), ForeignKey("fk_IssuePlace")]
        public long? IssuingPlaceId { get; set; }

        public virtual CityMaster fk_IssuePlace { get; set; }

        [Column("ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Column("ExpInYears")]
        public long? ExpInYears { get; set; }

        [Column("ExpDetails")]
        [MaxLength(200)]
        public string ExpDetails { get; set; }

        [Column("DPIN")]
        [MaxLength(100)]
        public string DPIN { get; set; }

        [Column("OpeningBalance")]
        public decimal OpeningBalance { get; set; } = 0;

        [Column("ReferenceName")]
        [MaxLength(100)]
        public string ReferenceName { get; set; }

        [Column("ReferenceAddress")]
        [MaxLength(200)]
        public string ReferenceAddress { get; set; }

        [Column("ReferenceMobileNo")]
        [MaxLength(20)]
        public string ReferenceMobileNo { get; set; }
        [Column("QualificationId")]
        public long? QualificationId { get; set; }
        [ForeignKey("QualificationId")]
        public virtual ConstantValue fk_Qualification { get; set; }
        [Column("CurrentAddressId")]
        public long? CurrentAddressId { get; set; }
        [ForeignKey("CurrentAddressId")]
        public virtual PostalAddress fk_CurrentAddress { get; set; }
        [Column("PermnAddress")]
        public long? PermanentAddressId { get; set; }
        [ForeignKey("PermanentAddressId")]
        public virtual PostalAddress fk_PermanentAddress { get; set; }
        public virtual List<DriverGuarantor> Guarantors { get; set; }
        public virtual List<DriverTrainingLog> TrainingLogs { get; set; }
        public virtual List<DriverPayment> Payments { get; set; }
        public virtual List<DriverRelative> Relatives { get; set; }

        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        public vwFleetAccount AccountDetail { get; set; }
        public List<DriverIncidentLog> IncidentLogs { get; set; }


        [Column("MarutiIDCard"),MaxLength(200)]
        public string MarutiIDCard { get; set; }

        [Column("DriverContactNo1"), MaxLength(25)]
        public string DriverContactNo1 { get; set; }     
        [Column("DriverContactNo2"), MaxLength(25)]
        public string DriverContactNo2 { get; set; }

        [Column("DriverEmailAdd"), MaxLength(100)]
        public string DriverEmailAdd { get; set; }
        [MaxLength(200)]
        public string Ref1 { get; set; }
        [MaxLength(200)]
        public string Ref2 { get; set; }
        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_RefI { get; set; }
        public long? Ref2Id { get; set; }
        [ForeignKey("Ref2Id")]
        public virtual GenericMaster fk_RefII { get; set; }
        public long? ViewId { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }

        [Column("AadhaarNo"), MaxLength(20)]
        public string AadhaarNo { get; set; }

        public decimal Salary { get; set; } = 0;

        public long? FleetManager1Id { get; set; }
        [ForeignKey("FleetManager1Id")]
        public virtual GenericMaster fk_FleetManager { get; set; }

        #region bank 1
        public long? Bank1Id { get; set; }
        [ForeignKey("Bank1Id")]
        public virtual GenericMaster fk_Bank1 { get; set; }

        public long? BankAc1Id { get; set; }
        [ForeignKey("BankAc1Id")]
        public virtual Ledger fk_BankAc1 { get; set; }

        [Column("BankAcccoutName1"), MaxLength(150)]
        public string BankAcccoutName1 { get; set; }

        [Column("BankAcccoutNo1"), MaxLength(100)]
        public string BankAcccoutNo1 { get; set; }

        [Column("BankCode1"), MaxLength(80)]
        public string BankCode1 { get; set; }
        [Column("BankSwiftCode1"), MaxLength(80)]
        public string BankSwiftCode1 { get; set; }

        [Column("BankUPI1"), MaxLength(15)]
        public string BankUPI1 { get; set; }

        [Column("BankAdd1"), MaxLength(1500)]
        public string BankAdd1 { get; set; }
        #endregion
        #region Account2 Outside currency bank
        public long? Bank2Id { get; set; }
        [ForeignKey("Bank2Id")]
        public virtual GenericMaster fk_Bank2 { get; set; }

        public long? BankAc2Id { get; set; }
        [ForeignKey("BankAc2Id")]
        public virtual Ledger fk_BankAc2 { get; set; }

        [Column("BankAcccoutName2"), MaxLength(150)]
        public string BankAcccoutName2 { get; set; }

        [Column("BankAcccoutNo2"), MaxLength(100)]
        public string BankAcccoutNo2 { get; set; }

        [Column("BankCode2"), MaxLength(80)]
        public string BankCode2 { get; set; }
        [Column("BankSwiftCode2"), MaxLength(80)]
        public string BankSwiftCode2 { get; set; }

        [Column("BankUPI2"), MaxLength(15)]
        public string BankUPI2 { get; set; }

        [Column("BankAdd2"), MaxLength(1500)]
        public string BankAdd2 { get; set; }
        #endregion

    }


}
