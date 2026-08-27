using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenant.Models
{
    public class TenantConnection
    {
        public string TenantId { get; set; }
        public string ConnectionString { get; set; }
    }
}
