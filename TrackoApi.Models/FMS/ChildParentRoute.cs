using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("mChildRoute")]
    public class ChildParentRoute:AuditableEntity
    {
        [Index("IX_ChildParentRoute_Unique", IsUnique = true,Order = 0)]
        public long ParentRouteId { get; set; }
        [ForeignKey("ParentRouteId")]
        public virtual RouteMaster fk_Parent { get; set; }
        [Index("IX_ChildParentRoute_Unique", IsUnique = true, Order = 1)]

        public long ChildRouteId { get; set; }
        [ForeignKey("ChildRouteId")]
        public virtual RouteMaster fk_Child { get; set; }

    }
}