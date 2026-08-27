using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class RouteVehicleTypeDbMapping : EntityTypeConfiguration<RouteVehicleType>
    {
        public RouteVehicleTypeDbMapping()
        {
          
            HasRequired(x => x.fk_VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId).WillCascadeOnDelete(false);
        }
    }
}
