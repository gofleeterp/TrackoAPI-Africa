using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global.DTS
{
    [Table("mDTSStatusMap")]
    public class DTSStatusMapping : AuditableEntity
    {
        public DTSStatusMapping()
        {
            Id = 0;
        }
        [Index("IDX_DTSStatusMapping_Unique",IsUnique = true,Order = 1)]
        public long CurrentStatusId { get; set; }
        [ForeignKey("CurrentStatusId")]
        public virtual DTSStatus fk_CurrentStatus { get; set; }
        [Index("IDX_DTSStatusMapping_Unique", IsUnique = true, Order = 2)]
        public long NextStatusId { get; set; }
        [ForeignKey("NextStatusId")]
        public virtual DTSStatus fk_NextStatus { get; set; }
        public bool IsReserved { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}
