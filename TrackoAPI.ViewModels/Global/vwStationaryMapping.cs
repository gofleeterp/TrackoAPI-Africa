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
    public class vwStationaryMapping
    {
        public long BookId { get; set; }
        public DateTime? IssueDate { get; set; }
        public long? OfficeId { get; set; }
        public string IssuedTo { get; set; }
        public long? ClientId { get; set; }
        public string MappingRemark { get; set; }
    }
}
