using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mBook")]
    public class StationeryBook : AuditableEntity
    {
        [Index("IDX_mBook_Name",IsUnique = true),MaxLength(300)]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the type identifier.
        /// ConstantTypeId 90
        /// eg. LR//Bill//Due//TyreIssue
        /// </summary>
        /// <value>The type identifier.</value>
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        [MaxLength(50,ErrorMessage = "Max Lenght of Prefix could be 50 chars")]
        public string Prefix { get; set; }
        public int StartingNumber { get; set; }
        public int NoOfDigits { get; set; }
        [Range(0,10000)]
        public int NoOfPages { get; set; }
        [MaxLength(200)]
        public string PreviousUsedPage { get; set; }
        public long NatureId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ nature.
        /// 1232:Auto,1233:Book,1234:Manual
        /// ConstantTypeId 84
        /// </summary>
        /// <value>The FK_ nature.</value>
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }
        [DataType(DataType.Date)]
        
        public DateTime? ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsLocked { get; set; }
        [DataType(DataType.Date)]
        public DateTime? AllotedDate { get; set; }
        [MaxLength(100)]
        public string IssueToPerson { get; set; }
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public long? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Ledger fk_Client { get; set; }
        [MaxLength(500)]
        public string BookRemark { get; set; }
        [MaxLength(500)]
        public string MappingRemark { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public string ExtraInfo { get; set; }
    }

    [Table("mBookLog")]
    public class StationeryBookLog :AuditableEntity
    {
        [MaxLength(200)]
        public string PageNo { get; set; }
        [DataType(DataType.Date)]
        public DateTime AllotedDate { get; set; }
        public bool IsUsed { get; set; }
        public long BookId { get; set; }
        [ForeignKey("BookId")]
        public virtual StationeryBook fk_StationeryBook { get; set; }
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        public long? NatureId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ nature.
        /// 1232:Auto,1233:Book,1234:Manual
        /// ConstantTypeId 84
        /// </summary>
        /// <value>The FK_ nature.</value>
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public long? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Ledger fk_Client { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public StationeryBookLog Clone()
        {
            return (StationeryBookLog) this.MemberwiseClone();
        }
    }
    [Table("mBookLogArchive")]
    public class StationeryBookLogArchive : AuditableEntity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [MaxLength(200)]
        public string PageNo { get; set; }
        [DataType(DataType.Date)]
        public DateTime AllotedDate { get; set; }
        public bool IsUsed { get; set; }
        public long BookId { get; set; }
        [ForeignKey("BookId")]
        public virtual StationeryBook fk_StationeryBook { get; set; }
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        public long? NatureId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ nature.
        /// 1232:Auto,1233:Book,1234:Manual
        /// ConstantTypeId 84
        /// </summary>
        /// <value>The FK_ nature.</value>
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public long? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Ledger fk_Client { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public bool IsCanceled { get; set; } = false;
        public DateTime? CanceledDated { get; set; } = null;
        [MaxLength(500)]
        public string CanceledRemark { get; set; }

        public long? CanceledByUserId { get; set; } = null;
    }
}
