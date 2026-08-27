using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    
    [Table("mOfficeMaster")]
    public class OfficeMaster : AuditableEntity
    {
        [Column("OfficeName"), Index("IDX_mOfficeMaster_OfficeName", IsUnique = true), Required, MaxLength(100)]
        public string OfficeName { get; set; }


        [Column("OfficeAbbr"), Index("IDX_mOfficeMaster_OfficeAbbr", IsUnique = true), Required, MaxLength(100)]
        public string OfficeAbbr { get; set; }

        [Column("CityId"), ForeignKey("fk_City")]
        public long? CityId { get; set; }
        public virtual CityMaster fk_City { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public virtual List<CityMaster> ControlledCities { get; set; }
        public long? AddressId { get; set; }
        [ForeignKey("AddressId")]
        public virtual PostalAddress fk_Address { get; set; }

        [MaxLength(1500)]
        public string PrintingAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;


        public long? DefaultCashAccountId { get; set; }
        [ForeignKey("DefaultCashAccountId")]
        public virtual Ledger fk_DefaultCashAc { get; set; }

        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_StateCode { get; set; }
        
        [MaxLength(200)]
        public string GSTNo { get; set; }

        /// <summary>
        /// Constant Type 138
        /// Constant Values 1626[RCM] and 1627[FCM]
        /// </summary>
        public long? GSTNatureId { get; set; }
        [ForeignKey("GSTNatureId")]
        public virtual ConstantValue fk_GSTNature { get; set; }

        /// <summary>
        /// Constant Type 149
        /// Constant Values 1739=Monthly,1740=Quaterly,1741=Yearly
        /// </summary>
        [ForeignKey("GSTR1PeriodicityId")]
        public virtual ConstantValue fk_GSTR1Periodicity { get; set; }
        public long? GSTR1PeriodicityId { get; set; }
        public DateTime? GSTR1ApplicableFrom { get; set; }
        public virtual List<LedgerOffice> Ledgers { get; set; }
    }
}
