using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Repairs;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.AMS
{
    [Table("mLedger")]
    public class Ledger : AuditableEntity,IValidatableObject
    {
        public Ledger()
        {
            ReferenceFlag = false;
            IsDefaulter = false;
            IsReserved = false;
        }
        [Column("AccountAbbr"), Required, MaxLength(100),Index("IX_Ledger_Alias",IsUnique = true)]
        public string Alias { get; set; }

        [Column("AccountName"), Required, MaxLength(150), Index("IX_Ledger_AccountName", IsUnique = true)]
        public string AccountName { get; set; }
        [Column("FleetAcName"), MaxLength(150)]
        public string FleetAcName { get; set; }

        [Column("BookingAcName"),MaxLength(150)]
        public string BookingAcName { get; set; }
        [Column("KnownAs1"),  MaxLength(150)]
        public string KnownAs1 { get; set; }

        public long? ParentCompanyId { get; set; }
        [ForeignKey("ParentCompanyId")]
        public virtual Ledger ParentCompany { get; set; }
        [Column("KnownAs2"),MaxLength(150)]
        public string KnownAs2 { get; set; }
        [Column("GroupId"),ForeignKey("fk_Group")]
        public long? GroupId { get; set; }
        public virtual AccountGroup fk_Group { get; set; }

        [Column("OfficeID"), ForeignKey("fk_Office"), Required]
        public long? OfficeId { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("IsAccountImpact")]
        public bool IsAccountImpact { get; set; }=true;

        [Column("AccountRoleID"),ForeignKey("fk_AccountRole")]
        public long? AccountRoleId { get; set; }
        public virtual ConstantValue fk_AccountRole { get; set; }
        [Column("PanNo"),MaxLength(10)]
        public string PanNo { get; set; }
        [Column("PrintingName"),MaxLength(255)]
        public string InvoicePrintingName { get; set; }

        [Column("TINNo"), MaxLength(50), Obsolete("TINNo has been depricated after announcement of GST",true)]
        public string TINNo { get; set; }

        [Column("STNo"), MaxLength(50), Obsolete("STNo has been depricated after announcement of GST", true)]
        public string STNo { get; set; }

        #region bank 1
        public long? Bank1Id { get; set; }
        [ForeignKey("Bank1Id")]
        public virtual GenericMaster fk_Bank1 { get; set; }

        public long? BankAc1Id { get; set; }
        [ForeignKey("BankAc1Id")]
        public virtual Ledger fk_BankAc1 { get; set; }

        [Column("BankAcccoutName1"), MaxLength(150)]
        public string BankAcccoutName1 { get; set; }

        [Column("BankAcccoutNo1"), MaxLength(100)]
        public string BankAcccoutNo1 { get; set; }

        [Column("BankCode1"), MaxLength(80)]
        public string BankCode1 { get; set; }
        [Column("BankSwiftCode1"), MaxLength(80)]
        public string BankSwiftCode1 { get; set; }

        [Column("BankUPI1"), MaxLength(15)]
        public string BankUPI1 { get; set; }

        [Column("BankAdd1"), MaxLength(1500)]
        public string BankAdd1 { get; set; }
        #endregion
        #region Account2 Outside currency bank
        public long? Bank2Id { get; set; }
        [ForeignKey("Bank2Id")]
        public virtual GenericMaster fk_Bank2 { get; set; }

        public long? BankAc2Id { get; set; }
        [ForeignKey("BankAc2Id")]
        public virtual Ledger fk_BankAc2 { get; set; }


        [Column("BankAcccoutName2"), MaxLength(150)]
        public string BankAcccoutName2 { get; set; }

        [Column("BankAcccoutNo2"), MaxLength(100)]
        public string BankAcccoutNo2 { get; set; }

        [Column("BankCode2"), MaxLength(80)]
        public string BankCode2 { get; set; }
        [Column("BankSwiftCode2"), MaxLength(80)]
        public string BankSwiftCode2 { get; set; }

        [Column("BankUPI2"), MaxLength(15)]
        public string BankUPI2 { get; set; }

        [Column("BankAdd2"), MaxLength(1500)]
        public string BankAdd2 { get; set; }
        #endregion

        //[Column("RateChartId"), ForeignKey("fk_RateChart")]
        //[Obsolete("This Member has been depricated, Instead use Contract ContractId")]
        //public long? RateChartId { get; set; }
        //[Obsolete("This Member has been depricated, Instead use Contract fk_Contract")]//TODO:Remove this and attached Member
        //public virtual GenericMaster fk_RateChart { get; set; }
        public long? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual CNRateContract fk_Contract { get; set; }

        

        [Column("CreditNatureId"),ForeignKey("fk_CreditNature")]
        public long? CreditNatureId {get; set;}
        public virtual ConstantValue fk_CreditNature { get; set; }

        public bool IsPodRequired { get; set; }

        [Column("CreditPeriod")]
        public long? CreditPeriod {get; set;}

        [Column("IsDefaulter")]
        public bool IsDefaulter { get; set; }
        [Column("ReferenceFlag")]
        public bool ReferenceFlag { get; set; }
        [Column("TDSDeclarationFlag")]
        public bool TDSDeclarationFlag { get; set; } = false;
        public long? AddressId { get; set; }
        [ForeignKey("AddressId")]
        public virtual PostalAddress fk_Address { get; set; }
        [MaxLength(200)]
        public string RefI { get; set; }
        [MaxLength(200)]
        public string RefII { get; set; }
        [MaxLength(200)]
        public string RefIII { get; set; }
        [MaxLength(200)]
        public string RefIV { get; set; }
        [MaxLength(200)]
        public string RefV { get; set; }

        [MaxLength(200)]
        public string BillRefI { get; set; }
        [MaxLength(200)]
        public string BillRefII { get; set; }

        [Column("EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }
        [Column("OpeningBalance")]
        public decimal OpeningBalance { get; set; } = 0;
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        public string FullName
        {
            get { return $"{AccountName}-[{Alias}]"; }
            set
            {
                //Ignore
            }
        }

        public virtual List<LedgerRole> Roles { get; set; }
        public virtual List<LedgerOffice> Offices { get; set; }
        public virtual List<MaterialMaster> Materials { get; set; }
        public List<Ledger> Subsidiaries { get; set; }
        public virtual List<SpareBinMapping> Bins { get; set; }

        #region GST Fields
        [Column("GSTIN"), MaxLength(100)]
        public string GSTIN { get; set; }
        public bool IsTaxApplicable { get; set; } = true;
        public long? TaxTypeId { get; set; }
        [ForeignKey("TaxTypeId")]
        public virtual ConstantValue fk_TaxType { get; set; }

        public long? ServiceTypeId { get; set; }
        [ForeignKey("ServiceTypeId")]
        public virtual TaxServiceType fk_ServiceType { get; set; }

        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_State { get; set; }
        [Column("BillingOfficeId"), ForeignKey("fk_BillOffice")]
        public long? BillingOfficeId { get; set; }
        public virtual OfficeMaster fk_BillOffice { get; set; }

        public long? SalesAccountId { get; set; }
        [ForeignKey("SalesAccountId")]
        public virtual Ledger fk_SalesAccount { get; set; }

        public long? UnbilledSalesAcId { get; set; }
        [ForeignKey("UnbilledSalesAcId")]
        public virtual Ledger fk_UnbilledSalesAccount { get; set; }
        #endregion
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DynamicProperties != null&&DynamicProperties.Count>0)
            {
                Data = JsonConvert.SerializeObject(DynamicProperties);
            }
            if (GSTNatureId == null)
            {
                GSTNatureId = 1670;
            }
            if (string.IsNullOrWhiteSpace(BookingAcName))
            {
                BookingAcName = AccountName;
            }
            if (string.IsNullOrWhiteSpace(FleetAcName))
            {
                FleetAcName = AccountName;
            }
            if (BillingOfficeId.GetValueOrDefault(0) == 0)
            {
                BillingOfficeId = OfficeId;
            }
            if (IsAccountImpact && GroupId.GetValueOrDefault(0) == 0)
            {
                yield return new ValidationResult("Account Group is Required",new []{ "GroupId" });
            }
            if (!IsAccountImpact && AccountRoleId.GetValueOrDefault(0) == 0)
            {
                yield return new ValidationResult("Account Role is Required", new[] { "AccountRoleId" });
            }
        }
        public long? ViewId { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }
        public int RoundUpDigit { get; set; } = 0;
        /// <summary>
        /// Constant Type 138
        /// Constant Values 1626[RCM] and 1627[FCM]
        /// </summary>
        public long? GSTNatureId { get; set; } = 1670;
        [ForeignKey("GSTNatureId")]
        public virtual ConstantValue fk_GSTNature { get; set; }

        public string Data { get; set; } = "[]";
        [Column("AutoCNEnabled")]
        public bool AutoCNEnabledOnTrip { get; set; } = false;

        public long? GeoLocationId { get; set; }
        [ForeignKey("GeoLocationId")]
        public virtual CityMaster fk_GeoLocation { get; set; }

        public long? LoadTypeId { get; set; }
        [ForeignKey("LoadTypeId")]
        public virtual LoadType fk_LoadType { get; set; }

        public long? SalesCategoryId { get; set; }
        [ForeignKey("SalesCategoryId")]
        public virtual ConstantValue fk_SalesCategory { get; set; }

        public long? MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual MaterialMaster fk_Material { get; set; }

        public long? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual UnitMaster fk_Unit { get; set; }

        public decimal CreditAmount { get; set; } = 0;

        public long? CreditCurTypeId { get; set; }
        [ForeignKey("CreditCurTypeId")]
        public virtual GenericMaster fk_CreditCurType { get; set; }


        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        public long? MISGroupId { get; set; }
        [ForeignKey("MISGroupId")]
        public virtual GenericMaster fk_MISGroup { get; set; }

        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> JsonDataList
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(JsonData)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData)): _dt;
            get
            {
                try
                {
                    if (Data == "{}") Data = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(Data ?? (Data = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                Data = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }


        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((Data ?? "{}") == "{}") Data = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((Data ?? (Data = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                Data = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                Data = "[]";
            }
        }
        public IDictionary<string,object> DynamicProperties { get; set; }
    }
}