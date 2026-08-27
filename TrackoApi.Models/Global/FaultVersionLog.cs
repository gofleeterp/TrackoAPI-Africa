using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Global
{
    [Table("tFaultVersions")]
    public class FaultVersionLog:Base.Entity
    {
        [Key]
        public override long Id { get => base.Id; set => base.Id = value; }
        [MaxLength(100)]
        public string FaultyVersionCode { get; set; }
        public long? ViewId { get; set; }
        [MaxLength(100)]
        public string NewVersionCode { get; set; }
        [MaxLength(500)]
        public string ErrorMessage { get; set; }
    }    
}
