using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.AMS
{
    public class vwVoucherEntryOpeningBalance
    {
        public bool HasBillByBillFlag { get; set; }
        public decimal ClosingBalance { get; set; }
        public DateTime AsOfDate{ get; set; }
    }
}
