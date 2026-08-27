using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mAPLConfig")]
    public class APLConfig : AuditableEntity
    {
        public APLConfig() {
            IsItemLevelAPL = false;
        }
        public bool IsItemLevelAPL { get; set; } = false;
        
        public long RoleId { get; set; }

        public long ViewId { get; set; }

        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_APLType { get; set; }

        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }

        public long PermissionLevelId { get; set; } = 0;

        public long? AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Ledger fk_Account { get; set; }

        public decimal Amount { get; set; } = 0;
        public decimal MinValue { get; set; } = 0;
        public decimal MaxValue { get; set; } = 0;


        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [Table("mAPLType")]
    public class APLType : AuditableEntity
    {
        public long ViewId { get; set; }

        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
    }

    [Table("tAPLLog")]
    public class APLLog : AuditableEntity
    {
        public long? APLRequestId { get; set; }
        [ForeignKey("APLRequestId")]
        public virtual JsonTransactionLog fk_APLRequest { get; set; }

        public string KeyValue { get; set; }
        public long? TransactionId { get; set; }
        public string TransactionNo { get; set; }
        public DateTime? TransactionDate { get; set; }
        public long? ViewId { get; set; }

        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_APLLogType { get; set; }

        public long? RoleId { get; set; }

        public long? GenRef1Id { get; set; }
        public long? GenRef2Id { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }

        public long? AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Ledger fk_Account { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public decimal Qty { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public string Remarks { get; set; }
        public int? ApprovedStatusId { get; set; } = 0;
        public long? ApprovalUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalRemarks { get; set; }
        public bool IsActive { get; set; } = true;
        public string BatchId { get; set; } 
    }

    [Table("tAPLLogAnx")]
    public class APLLogAnx : AuditableEntity
    {
        //[Column("TSLId"), Index("IDX_APLLogAnx_Unique", 1, IsUnique = true)]
        public long? TSLId { get; set; }
        
        //[Column("APLLogId"), Index("IDX_APLLogAnx_Unique", 2, IsUnique = true)]
        public long APLLogId { get; set; }
        [ForeignKey("APLLogId")]
        public virtual APLLog fk_APLLog { get; set; }

        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_APLLogAnxType { get; set; }

        public long? RecordId { get; set; }
        public string RecordNo { get; set; }

        public decimal Qty { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public string Remarks { get; set; }

        public decimal APLQty { get; set; } = 0;
        public decimal APLRate { get; set; } = 0;
        public decimal APLAmount { get; set; } = 0;
        public string APLRemarks { get; set; }
        public long? GenRef1Id { get; set; }
        [ForeignKey("GenRef1Id")]
        public virtual GenericMaster fk_GenRef1 { get; set; }
        public long? GenRef2Id { get; set; }
        [ForeignKey("GenRef2Id")]
        public virtual GenericMaster fk_GenRef2 { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string BatchId { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

       [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;

        public int? ApprovedStatusId { get; set; } = 0;
        public long? ApprovalUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalRemarks { get; set; }
    }

    [Table("tAPLLogAnxLevel")]
    public class APLLogAnxLevel : AuditableEntity
    {        
        public long APLAnxId { get; set; }
        [ForeignKey("APLAnxId")]
        public virtual APLLogAnx fk_APLAnx { get; set; }

        public long APLLogId { get; set; }
        [ForeignKey("APLLogId")]
        public virtual APLLog fk_APLLog { get; set; }

        public long APLConfigId { get; set; }
        [ForeignKey("APLConfigId")]
        public virtual APLConfig fk_APLConfig { get; set; }

        public int? ApprovedStatusId { get; set; } = 0;
        public long? ApprovalUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalRemarks { get; set; }
    }
}