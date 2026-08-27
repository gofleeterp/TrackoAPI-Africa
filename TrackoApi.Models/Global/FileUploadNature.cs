using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("mFileUploadNature")]
    public class FileUploadNature:AuditableEntity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(20)]
        public string Code { get; set; }
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public ConstantValue fk_Type { get; set; }
        [MaxLength(2000)]
        public string AllowedExtensions { get; set; }
        public long MaxFileSize { get; set; }
        public int MaxFilesPerRecord { get; set; }
        public MasterStatus Status { get; set; }
    }
}
