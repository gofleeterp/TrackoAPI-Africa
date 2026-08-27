using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.FMS.Battery
{
    public class vwBatteryPerformanceAgeUpdate
    {
        public long BatteryId { get; set; }
        public int Life { get; set; }
        public int CurrentAge { get; set; }
        public int LifeAge { get; set; }
        public int PreviousAge { get; set; }
    }
}
