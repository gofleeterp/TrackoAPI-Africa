using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

using TrackoAPI.ViewModels.AMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;

namespace TrackoApi.Models.AMS
{

    [Table("tVouchers")]
    public class Voucher : AuditableEntity, IValidatableObject
    {

        public Voucher()
        {
            Id = 0;
            VoucherDetails = new List<VoucherDetail>();
            VoucherDateTime = VoucherDate;
            IsAudited = false;
            IsAccountsVisiblity = true;
            IsParent = false;
            VDCount = 0;
            VdrJson = new List<FakeVDRs>();
        }
        [MaxLength(50)]
        public string TPT_RequestId { get; set; }
        public bool IsCCRequired { get; set; } = true;
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;

        [Column("OfficeId"), Required, ForeignKey("fk_Office")]
        public long OfficeId { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("VoucherNo"), StationaryCheck, Index("IX_Voucher_VoucherNo", IsUnique = true), Required, MaxLength(150), MinLength(2)]
        public string VoucherNo { get; set; }

        [Column("VoucherDate"), Required, DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; }

        [Column("VoucherDateTime"), DataType(DataType.DateTime)]
        //TODO:This Field should be deprecated in Future
        public DateTime? VoucherDateTime { get; set; }
        /// <summary>
        /// Gets or sets the payment mode identifier.
        /// ConstantTypeId 110
        /// </summary>
        /// <value>The payment mode identifier.</value>
        public long? PaymentModeId { get; set; }
        [ForeignKey("PaymentModeId")]
        public virtual ConstantValue fk_PaymentMode { get; set; }
        [MaxLength(500)]
        public string PaidTo { get; set; }

        [Column("VoucherTypeId"), Required, ForeignKey("FK_VoucherType")]
        public long VoucherTypeId { get; set; }
        public virtual VoucherType FK_VoucherType { get; set; }

        [Column("VchAmount"), Required]
        public decimal VoucherAmount { get; set; }

        [Column("VoucherAmount_MNC")]
        public decimal VoucherAmount_MNC { get; set; } = 0;

        [Column("VoucherAmount_FX")]
        public decimal VoucherAmount_FX { get; set; } = 0;

        [Column("Account1Id"), ForeignKey("Account1")]
        public long? Account1Id { get; set; }

        public virtual Ledger Account1 { get; set; }

        [Column("Amount1")]
        public decimal Amount1 { get; set; }

        [Column("Amount1_MNC")]
        public decimal Amount1_MNC { get; set; } = 0;

        [Column("Account2Id"), ForeignKey("Account2")]
        public long? Account2Id { get; set; }
        public virtual Ledger Account2 { get; set; }
        [Column("Amount2")]
        public decimal Amount2 { get; set; }

        [Column("Amount2_MNC")]
        public decimal Amount2_MNC { get; set; } = 0;

        [Column("Account3Id"), ForeignKey("Account3")]
        public long? Account3Id { get; set; }
        public virtual Ledger Account3 { get; set; }

        [Column("Amount3")]
        public decimal Amount3 { get; set; } = 0;

        [Column("Amount3_MNC")]
        public decimal Amount3_MNC { get; set; } = 0;


        [Column("Account4Id"), ForeignKey("Account4")]
        public long? Account4Id { get; set; }
        public virtual Ledger Account4 { get; set; }

        [Column("Amount4")]
        public decimal Amount4 { get; set; } = 0;

        [Column("Amount4_MNC")]
        public decimal Amount4_MNC { get; set; } = 0;

        [Column("Account5Id"), ForeignKey("Account5")]
        public long? Account5Id { get; set; }
        public virtual Ledger Account5 { get; set; }

        [Column("Amount5")]
        public decimal Amount5 { get; set; } = 0;

        [Column("Amount5_MNC")]
        public decimal Amount5_MNC { get; set; } = 0;

        [Column("Account6Id"), ForeignKey("Account6")]
        public long? Account6Id { get; set; }
        public virtual Ledger Account6 { get; set; }
        [Column("Amount6")]
        public decimal Amount6 { get; set; } = 0;

        [Column("Amount6_MNC")]
        public decimal Amount6_MNC { get; set; } = 0;

        [Column("Account7Id"), ForeignKey("Account7")]
        public long? Account7Id { get; set; }
        public virtual Ledger Account7 { get; set; }
        [Column("Amount7")]
        public decimal Amount7 { get; set; } = 0;

        [Column("Amount7_MNC")]
        public decimal Amount7_MNC { get; set; } = 0;


        [Column("Account8Id"), ForeignKey("Account8")]
        public long? Account8Id { get; set; }
        public virtual Ledger Account8 { get; set; }
        [Column("Amount8")]
        public decimal Amount8 { get; set; } = 0;

        [Column("Amount8_MNC")]
        public decimal Amount8_MNC { get; set; } = 0;

        [Column("Account9Id"), ForeignKey("Account9")]
        public long? Account9Id { get; set; }
        public virtual Ledger Account9 { get; set; }
        [Column("Amount9")]
        public decimal Amount9 { get; set; } = 0;

        [Column("Amount9_MNC")]
        public decimal Amount9_MNC { get; set; } = 0;

        [Column("Account10Id"), ForeignKey("Account10")]
        public long? Account10Id { get; set; }
        public virtual Ledger Account10 { get; set; }
        [Column("Amount10")]
        public decimal Amount10 { get; set; } = 0;

        [Column("Amount10_MNC")]
        public decimal Amount10_MNC { get; set; } = 0;

        [Column("UserRemarks"), MaxLength(1000)]
        public string UserRemark { get; set; }

        [Column("AccNarration"), MaxLength(1000)]
        public string AccountingRemark { get; set; }

        [Column("RefTransId")]
        public long? ReferenceTransactionId { get; set; }
        /// <summary>
        /// Gets and Sets if The Transaction has been audited or not
        /// </summary>
        [Column("IsAudited")]
        public bool IsAudited { get; set; }
        /// <summary>
        /// Gets and Sets whether Transaction has been Imported in Account
        /// </summary>
        public bool IsAccepted { get; set; }
        /// <summary>
        /// Gets and Sets whether Transaction Is Visible in Account
        /// Case 1:If Accounts is Application then true else false
        /// Case 2: If forcefully Voucher is marked not to visible in Accounts then false else true
        /// </summary>
        public bool IsAccountsVisiblity { get; set; }
        public bool IsParent { get; set; }
        public virtual List<VoucherAuditLog> VoucherAuditLogs { get; set; }

        [Column("FYID"), ForeignKey("fk_FinancialYear")]
        public long? FinancialYearId { get; set; }
        public virtual FinancialYear fk_FinancialYear { get; set; }

        public virtual List<VoucherDetail> VoucherDetails { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //VDCount = VoucherDetails.Count;
            var vhcamt = VoucherDetails.Where(x => x.Amount > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.Amount);
            if (vhcamt > 0)
            {
                VoucherAmount = vhcamt;
            }
            

            if (VoucherTypeId <= 0)
            {
                yield return new ValidationResult("Voucher Type is Required", new[] { "VoucherTypeId" });
            }
            //if ((Amount1 + Amount2 + Amount3 + Amount4 +
            //     Amount5 + Amount6 + Amount7 + Amount8 + Amount9 + Amount10) != 0)
            //{

            //    yield return new ValidationResult
            //  ($"Sum of Credit and Debit Amount does not match. \n Hint: Values are {Amount1} + {Amount2} + {Amount3} + {Amount4} +{Amount5} + {Amount6} + {Amount7} + {Amount8} + {Amount9} + {Amount10}");
            //}
            if ((VoucherDetails.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.Amount) != 0))
            {
                var message =
                    $"Sum Voucher Detail Amount does not match.\nHint: Voucher No {VoucherNo}, Amount :{VoucherAmount} and Cr={-VoucherDetails.Where(x => x.ObjectState != ObjectState.Deleted && x.Amount <= 0).Sum(x => x.Amount)} and Dr={VoucherDetails.Where(x => x.ObjectState != ObjectState.Deleted && x.Amount > 0).Sum(x => x.Amount)}";
                foreach (var detail in VoucherDetails)
                {
                    message += $"\nAcId>{detail.AccountId}:Order>{detail.OrderId}:Amt>{detail.Amount}";
                }
                yield return new ValidationResult
              (message, new[]
              {
                  "VoucherDetails"
              });
            }
            if ((VoucherDetails?.Count ?? 0) > 0)
            {
                VDCount = VoucherDetails?.Count ?? 0;
            }

            //if (FinancialYearId.GetValueOrDefault(0) <= 0)
            //{
            //    yield return new ValidationResult($"Missing Finaincial Year for Voucher {VoucherNo}");
            //}
        }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public long? FileTypeId { get; set; }
        [ForeignKey("FileTypeId")]
        public virtual GenericMaster fk_FileType { get; set; }
        [MaxLength(200)]
        public string FileNo { get; set; }
        public long? ViewId { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }

        public long? AuditSessionId { get; set; }
        [MaxLength(500)]
        public string AuditRemark { get; set; }

        public long? ApprovalSessionId { get; set; }

        public DateTime? ApprovalMDOE { get; set; }

        [MaxLength(500)]
        public string ApprovalRemark { get; set; }

        public Voucher Clone()
        {
            return (Voucher)this.MemberwiseClone();
        }

        public int VDCount { get; set; } = 0;
        public long? GroupVoucherId { get; set; }
        [ForeignKey("GroupVoucherId")]
        public virtual Voucher fk_GroupVoucher { get; set; }
        public long? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Voucher fk_Parent { get; set; }
        public virtual List<Voucher> ChildVouchers { get; set; }
        public bool RemoveVD { get; set; } = false;

        public string GSTR2AUploadRefNo { get; set; }
        public long? GSTR2AUploadUserId { get; set; }
        [ForeignKey("GSTR2AUploadUserId")]
        public virtual ApiUser fk_GSTR2AUploadUser { get; set; }
        public DateTime? GSTR2AUploadDate { get; set; }
        public bool IsGSTExcluded { get; set; }
        public DateTime? GSTR1FinalDate { get; set; }
        public long? GSTR1FinalUserId { get; set; }
        [ForeignKey("GSTR1FinalUserId")]
        public virtual ApiUser fk_GSTR1FinalUser { get; set; }

        public long? GSTChallanVoucherId { get; set; }
        [ForeignKey("GSTChallanVoucherId")]
        public virtual Voucher fk_GSTChallanVoucher { get; set; }

        public string JsonData { get; set; }
        public List<FakeVDRs> VdrJson { get; set; }

        [Column("ChequeNo"), MaxLength(50)]
        public string ChequeNo { get; set; }
        [Column("ChequeId")]
        public long? ChequeId { get; set; }
        [Column("ChequeDate")]
        public DateTime? ChequeDate { get; set; }
        [Column("RefNo"), MaxLength(250)]
        public string RefNo { get; set; }
        [Column("RefDate")]
        public DateTime? RefDate { get; set; }

        [Column("ReasonId")]
        public long? ReasonId { get; set; }
        [ForeignKey("ReasonId")]
        public virtual GenericMaster fk_Reason { get; set; }

        [MaxLength(500)]
        public string OtherReason { get; set; }

        public int PrintCount { get; set; } = 0;

        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(ExtraProperties)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties)): _dt;
            get
            {
                try
                {
                    if (JsonData == "{}") JsonData = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData ?? (JsonData = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                JsonData = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }


        }
        private bool isFirstCall = true;
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((JsonData ?? "{}") == "{}") JsonData = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((JsonData ?? (JsonData = "[]")));
                }
                else if (isFirstCall)
                {
                    try
                    {
                        var _existingdt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((JsonData ?? (JsonData = "[]")));
                        if (_existingdt != null)
                        {
                            foreach (var item in _existingdt)
                            {
                                if (!_dt.Any(x => x.DataName == item.DataName))
                                {
                                    _dt.Add(item);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //Ignore
                    }
                    isFirstCall = false;
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                JsonData = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                JsonData = "[]";
            }
        }
    }
}