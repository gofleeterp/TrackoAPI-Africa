using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.FMS.Tyres
{
    public class vwTyrePerformanceKmUpdate
    {
        public long TyreId { get; set; }
        public string Source { get; set; }
        public int Life { get; set; }
        public int CurrentMilage { get; set; }
        public int LifeMilage { get; set; }
        public int PreviousMilage { get; set; }
    }
}
