using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tRouteVehicleType")]
    public class RouteVehicleType : AuditableEntity
    {
        [Column("RouteId"), ForeignKey("fk_Route")]
        public long RouteId { get; set; }
        public virtual RouteMaster fk_Route { get; set; }

        [Column("VehicleTypeId"), ForeignKey("fk_VehicleType")]
        public long VehicleTypeId { get; set; }
        public virtual GenericMaster fk_VehicleType { get; set; }
    }
}
