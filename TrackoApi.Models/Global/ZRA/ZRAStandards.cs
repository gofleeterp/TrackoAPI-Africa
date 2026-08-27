using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("ZRAClassificationCode")]
    public class ZRAClassificationCode
    {
        //public long Id { get; set; }

        [Key, MaxLength(25)]
        public string itemClsCd { get; set; }
        
        [MaxLength(255)]
        public string itemClsNm { get; set; }
        /// <summary>
        /// ZRA_ClassificationCodes
        /// </summary>
        
        [MaxLength(50)]
        public string itemClsLvl { get; set; }
        
        [MaxLength(50)]
        public string taxTyCd { get; set; }
        
        [MaxLength(5)]
        public string mjrTgYn { get; set; }
        
        [MaxLength(5)]
        public string useYn { get; set; }
    }
    [Table("ZRAStandard")]
    public class ZRAStandard
    {
        [Key,MaxLength(25)]
        public string cdCls { get; set; }
        
        [MaxLength(150)]
        public string cdClsNm { get; set; }
        /// <summary>
        /// ZRA_ClassificationCodes
        /// </summary>
        [MaxLength(50)]
        public string userDfnNm1 { get; set; }
    }
    [Table("ZRAStandardCode")]
    public class ZRAStandardCode: Entity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        [MaxLength(25), Index("IX_ZRA_StandardCodes_Unique", IsUnique = true, Order = 1)]
        public string cd { get; set; }
        [MaxLength(150), Index("IX_ZRA_StandardCodes_Unique", IsUnique = true, Order = 2)]
        public string cdNm { get; set; }


        [MaxLength(25), Index("IX_ZRA_StandardCodes_Unique", IsUnique = true, Order = 3)]
        public string cdCls { get; set; }
        [ForeignKey("cdCls")]
        public virtual ZRAStandard fk_cdCls { get; set; }

        [MaxLength(150)]
        public string userDfnCd1 { get; set; }
    }
}
