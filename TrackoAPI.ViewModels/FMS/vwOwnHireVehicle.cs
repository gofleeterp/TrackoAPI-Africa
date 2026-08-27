using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.FMS
{
    public class vwOwnHireVehicle
    {
        public long Id { get; set; }
        public string VehicleNo { get; set; }
        public string Type { get; set; }
        public long? OwnerId { get; set; }
        public string Owner { get; set; }
        public string RegistrationNo { get; set; }
    }
}
