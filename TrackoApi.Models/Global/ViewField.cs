using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.AMS
{
    [Table("mViewField")]
    public class ViewField:AuditableEntity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [MaxLength(100)]
        public string FieldType { get; set; }
        public long? DefaultGroupId { get; set; }
        [ForeignKey("DefaultGroupId")]
        public virtual AccountGroup fk_DefaultGroup { get; set; }
        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual  VoucherType fk_VoucherType { get; set; }
        public long? DefaultRoleId { get; set; }
        [ForeignKey("DefaultRoleId")]
        public virtual ConstantValue fk_DefaultRole { get; set; }
        //public string FixedIncludes { get; set; }
        //public bool IsReadOnly { get; set; }
        [Column("StatusId")]
        public MasterStatus Status { get; set; }

        public virtual List<VoucherTypeGroupMapping> Mappings { get; set; }
        public long? DefaultLedgerId { get; set; }
        [ForeignKey("DefaultLedgerId")]
        public virtual Ledger fk_DefaultLedger { get; set; }

        public long? ViewId { get; set; }
        [ForeignKey("ViewId")]
        public virtual ApiView fk_View { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }
        [MaxLength(50)]
        public string Label { get; set; }
        [MaxLength(500)]
        public string LabelToolTip { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReserved { get; set; }
        [MaxLength(15)]
        public string Watermark { get; set; }
        public long? BookTypeId { get; set; }
        [ForeignKey("BookTypeId")]
        public virtual ConstantValue fk_BookType { get; set; }

        public virtual List<ViewFieldBookMap> BookMaps { get; set; }
        public bool ShowInVTG { get; set; }
        [MaxLength(200)]
        public string ControlId { get; set; }
    }
    [Table("mViewFieldBookMap")]
    public class ViewFieldBookMap : AuditableEntity
    {
        public long FieldId { get; set; }
        [ForeignKey("FieldId")]
        public virtual ViewField fk_Field { get; set; }
        public long ViewId { get; set; }
        [ForeignKey("ViewId")]
        public virtual ApiView fk_View { get; set; }
        public long BookTypeId { get; set; }
        [ForeignKey("BookTypeId")]
        public virtual ConstantValue fk_BookType { get; set; }
        public long? NatureId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ nature.
        /// 1232:Auto,1233:Book,1234:Manual
        /// ConstantTypeId 84
        /// </summary>
        /// <value>The FK_ nature.</value>
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }
        /// <summary>
        /// Extra Type eg. VoucherTypeId or any other identifier to categorize mapping
        /// </summary>
        public long? TypeId { get; set; }
    }
}