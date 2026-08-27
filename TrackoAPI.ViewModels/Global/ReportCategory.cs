using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm.Library;
using System.Data.Entity.Core.Objects.DataClasses;

namespace TrackoAPI.ViewModels.Global
{
    [EdmComplexType]
    public class vwReportCategory
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }

        public long CategoryTypeId { get; set; }
        public string CategoryType { get; set; }

        public long RoleTypeId { get; set; }
        public  string RoleType { get; set; }

        public long? RoleId { get; set; }
        public string Role{ get; set; }
        public bool IsReserved { get; set; } = false;
    }
}
