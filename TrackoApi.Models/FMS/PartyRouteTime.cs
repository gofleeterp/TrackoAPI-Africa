using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mPartyRouteTime")]
    public class PartyRouteTime : AuditableEntity
    {
        [Column("PartyId"), Required, Index("IX_mPartyRouteTime_Unique", IsUnique = true, Order = 1)]
        public long PartyId { get; set; }
        [ForeignKey("PartyId")]
        public virtual Ledger fk_Party { get; set; }

        [Column("RouteId"), Required, Index("IX_mPartyRouteTime_Unique", IsUnique = true, Order = 2)]
        public long RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }

        [Column("TimeInHr")]
        public decimal TimeInHr { get; set; } = 0;

        [Column("RefId")]
        public long? RefId { get; set; }
        [ForeignKey("RefId")]
        public virtual GenericMaster fk_Ref { get; set; }

        public DateTime? EffectiveDate { get; set; }

        [Column("KmRun")]
        public decimal KmRun{ get; set; } =0;
    }
}
