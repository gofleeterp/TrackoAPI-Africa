using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tTollRateLog")]
    public class TollRateLog : AuditableEntity
    {
        [Column("LogDate"), Required]
        public DateTime LogDate { get; set; }


        [Column("VehicleAxleTypeId"), Required, ForeignKey("fk_AxleType")]
        public long VehicleAxleTypeId { get; set; }
        public virtual ConstantValue fk_AxleType { get; set; }

        [Column("TollRate"), Required]
        public decimal TollRate { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}
