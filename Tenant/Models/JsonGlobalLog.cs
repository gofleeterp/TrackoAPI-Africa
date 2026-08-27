using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models
{
    [Table("tJsonLog")]
    public class JsonGlobalLog
    {
        [Key,MaxLength(150),Index("IX_JsonLog_KeyPrefix",IsClustered = false,IsUnique = false)]
        [Column(Order = 1)]
        public string KeyPrefix { get; set; }
        [Key, MaxLength(150)]
        [Column(Order = 2)]
        public string JsonKey { get; set; }
        public string JsonData { get; set; }
    }
}
