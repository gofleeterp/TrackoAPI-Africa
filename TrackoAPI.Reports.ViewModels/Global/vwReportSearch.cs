using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.Reports.ViewModels.Global
{
    public class vwReportSearch
    {
        public long Id { get; set; }
        public string ReportName { get; set; }
        public string IsUDR { get; set; } = "N";
    }
}
