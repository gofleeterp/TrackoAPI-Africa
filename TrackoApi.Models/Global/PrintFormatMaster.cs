using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mPrtFrmtMaster")]
    public class PrintFormatMaster:Base.Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key]
        public override long Id { get; set; }
        [Index("IDX_PrintFormatMaster_DisplayText",IsUnique = true),MaxLength(200)]
        public string DisplayText { get; set; }
        public bool IsDefault { get; set; } = false;
        public long? ViewId { get; set; }
        [ForeignKey("ViewId")]
        public virtual ApiView fk_View { get; set; }
        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public ConstantValue fk_Type { get; set; }
        public bool IsReserved { get; set; } = false;
        [MaxLength(200)]
        public string ClientDisplayText { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual List<PrintFormatDataSource> DataSources { get; set; }
        public virtual List<LedgerPrintFormat> LedgerPrintFormats { get; set; }
        
    }
    [Table("mPrtFrmtDataSource")]
    public class PrintFormatDataSource : Base.Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None),Key]
        public override long Id { get; set; }
        public long PrintFormatId { get; set; }
        [ForeignKey("PrintFormatId")]
        public virtual PrintFormatMaster fk_PrintFormat { get; set; }
        [MaxLength(100)]
        public string DataSourceName { get; set; }
        [MaxLength(400)]
        public string Path { get; set; }
        public long? ProcId { get; set; }
        public bool IsSubDataSource { get; set; } = false;
        [NotMapped]
        public new ObjectState ObjectState { get; set; }
    }
    [Table("mPrtFrmtLedger")]
    public class LedgerPrintFormat:Base.AuditableEntity
    {
        public long PrintFormatId { get; set; }
        [ForeignKey("PrintFormatId")]
        public virtual PrintFormatMaster fk_PrintFormat { get; set; }

        public long? LedgerId { get; set; }
        [ForeignKey("LedgerId")]
        public virtual Ledger fk_Ledger { get; set; }

        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        public bool IsDefault { get; set; } = false;

        public long? AnnexureFormatId { get; set; }
        [ForeignKey("AnnexureFormatId")]
        public virtual PrintFormatMaster fk_AnnexureFormat { get; set; }
    }
}
