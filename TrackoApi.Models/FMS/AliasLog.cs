using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mAliasLog")]
    public class AliasLog : AuditableEntity
    {
        [Column("MasterRecordId"), Required]
        public long MasterRecordId { get; set; }
        
        [Column("TypeId"), ForeignKey("fk_Type"), Required]
        public long TypeId { get; set; }//Vehicle//Party//Route//City//MaterialMaster//MaterialGroup etc

        public virtual ConstantValue fk_Type { get; set; }

        [Column("KnownAsI"), Required, MaxLength(50)]
        public string KnownAsI { get; set; }

        [Column("KnownAsII"), Required, MaxLength(50)]
        public string KnownAsII { get; set; }

        [Column("KnownAsIII"), Required, MaxLength(50)]
        public string KnownAsIII { get; set; }

        [Column("TPICI"), Required, MaxLength(50)]
        public string ThirPartyIntegrationCodeI { get; set; }

        [Column("TPICII"), Required, MaxLength(50)]
        public string ThirPartyIntegrationCodeII { get; set; }

        [Column("TPICIII"), Required, MaxLength(50)]
        public string ThirPartyIntegrationCodeIII { get; set; }


        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

    }
}