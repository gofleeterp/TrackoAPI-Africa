using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.BMS
{
    [Table("mMaterialGroup")]
    public class MaterialGroup:AuditableEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Code { get; set; }
        public virtual List<MaterialMaster> Materials { get; set; }
    }

}