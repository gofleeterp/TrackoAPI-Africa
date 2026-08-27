using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tGTrans")]
    public class GeneralTransaction: GeneralTranAcDetl
    {
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        [MaxLength(200),Index("IDX_GeneralTransactionDocNo",IsUnique =true)]
        public string DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public int? TransacCategoryId { get; set; }
        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }

        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public DateTime? VoucherDate { get; set; }
        [MaxLength(200)]
        public string Ref1 { get; set; }
        [MaxLength(200)]
        public string Ref2 { get; set; }
        [MaxLength(200)]
        public string Ref3 { get; set; }
        [MaxLength(200)]
        public string Ref4 { get; set; }
        [MaxLength(200)]
        public string Ref5 { get; set; }
        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_Ref1 { get; set; }
        public long? Ref2Id { get; set; }
        [ForeignKey("Ref2Id")]
        public virtual GenericMaster fk_Ref2 { get; set; }
        public long? Ref3Id { get; set; }
        [ForeignKey("Ref3Id")]
        public virtual GenericMaster fk_Ref3 { get; set; }
        public long? ConstRef1Id { get; set; }
        [ForeignKey("ConstRef1Id")]
        public virtual ConstantValue fk_ConstRef1 { get; set; }
        public long? ConstRef2Id { get; set; }
        [ForeignKey("ConstRef2Id")]
        public virtual ConstantValue fk_ConstRef2 { get; set; }
        public long? ViewId { get; set; }
        public string Remark { get; set; }
        public virtual List<GeneralTransLog> Logs { get; set; } = new List<GeneralTransLog>();

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
    }
    [Table("tGTransLog")]
    public class GeneralTransLog : GeneralTranAcDetl
    {
        public long? GenTranId { get; set; }
        [ForeignKey("GenTranId")]
        public virtual GeneralTransaction fk_GenTran { get; set; }
        [MaxLength(200)]
        public string RefNo { get; set; }
        public long? TransactionType { get; set; }
        public long? RecordId { get; set; }        
        public string Remark { get; set; }
        
    }
    public abstract class GeneralTranAcDetl:AuditableEntity
    {
        public long? Account1Id { get; set; }
        [ForeignKey("Account1Id")]
        public virtual Ledger fk_Account1 { get; set; }
        public long? Account2Id { get; set; }
        [ForeignKey("Account2Id")]
        public virtual Ledger fk_Account2 { get; set; }
        public long? Account3Id { get; set; }
        [ForeignKey("Account3Id")]
        public virtual Ledger fk_Account3 { get; set; }
        public long? Account4Id { get; set; }
        [ForeignKey("Account4Id")]
        public virtual Ledger fk_Account4 { get; set; }
        public long? Account5Id { get; set; }
        [ForeignKey("Account5Id")]
        public virtual Ledger fk_Account5 { get; set; }
        public long? Account6Id { get; set; }
        [ForeignKey("Account6Id")]
        public virtual Ledger fk_Account6 { get; set; }
        public decimal Amount1 { get; set; }
        public decimal Amount2 { get; set; }
        public decimal Amount3 { get; set; }
        public decimal Amount4 { get; set; }
        public decimal Amount5 { get; set; }
        public decimal Amount6 { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(JsonData)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData)): _dt;
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
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((JsonData ?? "{}") == "{}") JsonData = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((JsonData ?? (JsonData = "[]")));
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
