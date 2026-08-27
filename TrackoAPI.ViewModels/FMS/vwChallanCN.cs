using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.FMS
{
    public class vwChallanCN
    {
        public long CNId { get; set; }
        public long ChallanId { get; set; }
        public decimal Qty { get; set; } = 0;
        public decimal Excess { get; set; } = 0;
        public decimal Short { get; set; } = 0;
        public decimal MarketFreight { get; set; } = 0;
        public decimal Revenue { get; set; } = 0;
        public decimal Weight { get; set; }
        public long? TriplogId { get; set; }
        public virtual vwCnChallanCharges CnChallanCnCharges { get; set; }
    }
    public class vwCnChallanCharges
    {
        public long Id { get; set; }
        public long CNId { get; set; }
        public decimal Detention { get; set; } = 0;
        public decimal OtherCharges { get; set; } = 0;
        public decimal UnloadCharges { get; set; } = 0;
        public decimal Penalty { get; set; } = 0;
        public decimal Claims { get; set; } = 0;
        public decimal MiscChg1 { get; set; } = 0;
        public decimal MiscChg2 { get; set; } = 0;
        public decimal OtherDed { get; set; } = 0;
    }
}
