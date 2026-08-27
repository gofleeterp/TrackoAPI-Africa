using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace TrackoApi.Models.Global
{
    [Table("tTransSupportLog")]
    public class TransactionSupportLog : AuditableEntity
    {
        [Key]
        public override long Id { get; set; }
        public long ViewId { get; set; }
        public long RecordId { get; set; }
        [MaxLength(200)]
        public string KeyValue { get; set; }
        [NotMapped]
        public string TextValue1 { get; set; }
        [NotMapped]
        public string TextValue2 { get; set; }
        public DateTime? DocDate { get; set; }

        public long? Ref1Id { get; set; }
        public long? Ref2Id { get; set; }

        [Column("Const1Id"), ForeignKey("fk_ConstI")]
        public long? Const1Id { get; set; }
        public virtual ConstantValue fk_ConstI { get; set; }

        [Column("Const2Id"), ForeignKey("fk_ConstII")]
        public long? Const2Id { get; set; }
        public virtual ConstantValue fk_ConstII { get; set; }

        [Column("Generic1Id"), ForeignKey("fk_GenericI")]
        public long? Generic1Id { get; set; }
        public virtual GenericMaster fk_GenericI { get; set; }

        [Column("Generic2Id"), ForeignKey("fk_GenericII")]
        public long? Generic2Id { get; set; }
        public virtual GenericMaster fk_GenericII { get; set; }

        public string RefI { get; set; }
        public string RefII { get; set; }

        public decimal Value1 { get; set; } = 0;
        public decimal Value2 { get; set; } = 0;
        public decimal Value3 { get; set; } = 0;
        public string JsonData { get; set; }
        public string Remarks { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;
    }

}