using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.Global
{
    public class CronQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required(AllowEmptyStrings =false),MinLength(9)]
        public string Cron { get; set; }
    }
}
