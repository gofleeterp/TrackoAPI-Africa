using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mRouteTollMap")]
    public class RouteTollMap : AuditableEntity
    {
        [Column("RouteId"), Required, Index("IDX_mRouteTollMap_Name", 1, IsUnique = true), ForeignKey("fk_Route")]
        public long RouteId { get; set; }
        public virtual RouteMaster fk_Route { get; set; }

        [Column("TollId"), Required, Index("IDX_mRouteTollMap_Name", 2, IsUnique = true), ForeignKey("fk_Toll")]
        public long TollId { get; set; }
        public virtual TollMaster fk_Toll { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }

}
