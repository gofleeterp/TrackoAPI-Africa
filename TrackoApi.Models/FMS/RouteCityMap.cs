using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mRouteCityMap")]
    public class RouteCityMap : AuditableEntity
    {
        [Column("RouteId"), Required, Index("IDX_mRouteCityMap_Name",1, IsUnique = true), ForeignKey("fk_Route")]
        public long RouteId { get; set; }
        public virtual RouteMaster fk_Route { get; set; }

        [Column("CityId"), Required, Index("IDX_mRouteCityMap_Name", 2, IsUnique = true), ForeignKey("fk_City")]
        public long CityId { get; set; }
        public virtual CityMaster fk_City { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}
