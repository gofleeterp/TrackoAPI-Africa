using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Global
{
    public class PermissionSet
    {
        public long ApiObjectId { get; set; }
        public int EffectivePermission { get; set; }
        public AccessType EntityType { get; set; }
        public long UserId { get; set; }
        public string ObjectName { get; set; }
        public long Id { get; set; }
    }
}
