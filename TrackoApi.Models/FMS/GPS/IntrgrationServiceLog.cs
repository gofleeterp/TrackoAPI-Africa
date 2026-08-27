using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS.GPS
{
    [Table("mIntrgrationServiceLog")]
    public class IntrgrationServiceLog:AuditableEntity
    {
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual ConstantValue fk_Service { get; set; }
        public long ServiceProviderId { get; set; }
        [ForeignKey("ServiceProviderId")]
        public virtual Ledger fk_ServiceProvider { get; set; }
        public long StateCounter { get; set; } = 0;

    }
    
}
