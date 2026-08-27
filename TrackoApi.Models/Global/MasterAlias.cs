using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("mAlias")]
    public class MasterAlias : AuditableEntity,IValidatableObject
    {
        [Column("AliasName"), MaxLength(200), Index("IDX_mMasterAlias_Unique", 1, IsUnique = true)]
        public string AliasName { get; set; }

        [Column("ReletedId"), Index("IDX_mMasterAlias_Unique", 2, IsUnique = true)]
        public long RelatedId { get; set; }
        /// <summary>
        /// Gets or sets the related type identifier.
        /// Constant Type Id would be 51
        /// </summary>
        /// <value>The related type identifier.</value>
        [Index("IDX_mMasterAlias_Unique", 3, IsUnique = true)]
        public long RelatedTypeId { get; set; }
        [ForeignKey("RelatedTypeId")]
        public virtual ConstantValue fk_RelatedType { get; set; }
        /// <summary>
        /// Gets or sets the ext application identifier.
        /// Constant Type Id would be 71
        /// </summary>
        /// <value>The ext application identifier.</value>
        public long ExtAppId { get; set; }
        [ForeignKey("ExtAppId")]
        public virtual ConstantValue fk_ExternalApp { get; set; }

        public long? SpareItemId { get; set; }
        [ForeignKey("SpareItemId")]
        public SpareMaster fk_SpareItem { get; set; }
        [MaxLength(200)]
        public string Ref1 { get; set; }
        public MasterStatus Status { get; set; } = MasterStatus.Active;
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SpareItemId > 0)
            {
                RelatedId = SpareItemId.GetValueOrDefault();
            }
            else if (RelatedTypeId == 1072 && RelatedId > 0)
            {
                SpareItemId = RelatedId;
            }
            if (string.IsNullOrWhiteSpace(AliasName))
            {
                this.Status=MasterStatus.Suspended;
            }
            if (string.IsNullOrWhiteSpace(AliasName)&&Status==MasterStatus.Active)
            {
                yield return new ValidationResult("Alias Name is Required");
            }
        }
    }
}
