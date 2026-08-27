using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mTollMaster")]
    public class TollMaster : AuditableEntity
    {
        public TollMaster()
        {
            TollRateLogs = new List<TollRateLog>();
        }

        [Column("Name"),MaxLength(100), Required, Index("IDX_mTollMaster_Name", 1, IsUnique = true)]
        public string Name { get; set; }

        [Column("OperatorName")]
        [MaxLength(100)]
        public string OperatorName { get; set; }
        

        [Column("CityId"), Required, ForeignKey("fk_City")]
        public long CityId { get; set; }
        public virtual CityMaster fk_City { get; set; }

        [Column("StartDate"), Required]
        public DateTime StartDate { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public virtual ICollection<TollRateLog> TollRateLogs { get; set; }
        public long? ViewId { get; set; }
    }
}
