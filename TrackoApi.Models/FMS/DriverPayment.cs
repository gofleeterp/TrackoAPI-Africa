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
    [Table("tDriverPayment")]
    public class DriverPayment : AuditableEntity
    {
        [Column("OfficeId"), ForeignKey("fk_Office"), Required]
        public long OfficeId { get; set; }

        public virtual OfficeMaster fk_Office { get; set; }

        [Column("VoucherTypeId"), ForeignKey("fk_VoucherType"), Required]
        public long VoucherTypeId { get; set; }

        public virtual ConstantValue fk_VoucherType { get; set; }

        [Column("VoucherDate"), Required]
        public DateTime VoucherDate { get; set; }

        [Column("ReferenceNo"), StationaryCheck, Required, MaxLength(50)]
        public string ReferenceNo { get; set; }

        [Column("VehicleId"), Required, ForeignKey("fk_Vehicle")]
        public long VehicleId { get; set; }

        public VehicleMaster fk_Vehicle { get; set; }

        [Column("DriverId"), Required, ForeignKey("fk_Driver")]
        public long DriverId { get; set; }

        public DriverMaster fk_Driver { get; set; }

        [Column("DrAccountId"), Required, ForeignKey("fk_DrAccount")]
        public long DrAccountId { get; set; }

        public Ledger fk_DrAccount { get; set; }

        [Column("CrAccountId"), Required, ForeignKey("fk_CrAccount")]
        public long CrAccountId { get; set; }

        public Ledger fk_CrAccount { get; set; }

        public decimal PaymentAmount { get; set; }
        [MaxLength(300)]
        public string Remarks { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
    }
}
