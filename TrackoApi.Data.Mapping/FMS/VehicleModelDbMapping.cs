using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    class VehicleModelDbMapping : EntityTypeConfiguration<VehicleModel>
    {
        public VehicleModelDbMapping()
        {
            //HasRequired(x => x.fk_Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).WillCascadeOnDelete(false);
        }
    }
}
