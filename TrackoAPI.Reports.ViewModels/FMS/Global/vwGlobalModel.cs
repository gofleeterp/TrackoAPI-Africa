using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.Reports.ViewModels.FMS.Global
{
    public class vwCategoryClassMap
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public string ObjectName { get; set; }
        public long CategoryTypeId { get; set; }
        public long RoleId { get; set; }
        public long CategoryId { get; set; }
        public long ClassId { get; set; }
        public long ObjectId { get; set; }
        public long RoleTypeId { get; set; }
    }
}
