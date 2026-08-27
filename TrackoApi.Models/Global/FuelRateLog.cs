using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mFuelRateLog")]
    public class FuelRateLog : AuditableEntity
    {
        [Index("IDX_mFuelRateLogMap_Unique", IsUnique = true, Order = 1)]
        [Column("PumpId")]
        public long PumpId { get; set; }
        [ForeignKey("fk_Pump")]
        public virtual Ledger fk_Pump { get; set; }


        [Column("FuelId"), ForeignKey("fk_Fuel"), Index("IDX_mFuelRateLogMap_Unique", IsUnique = true, Order = 2), Required]
        public long FuelId { get; set; }
        public virtual GenericMaster fk_Fuel { get; set; }


        public decimal FuelRate { get; set; } = 0;

        [Index("IDX_mFuelRateLogMap_Unique", IsUnique = true, Order = 3)]
        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; } = null;
        [MaxLength(300)]
        public string Remark { get; set; }
        [MaxLength(150)]
        public string BatchId { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }

    }
}
