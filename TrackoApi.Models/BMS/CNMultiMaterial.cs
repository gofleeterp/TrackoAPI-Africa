using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tCNMultiMaterial")]
    public class CNMultiMaterial : AuditableEntity
    {
        [Column("InvoiceNo"), MaxLength(200)]
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        
        public decimal InvoiceValue { get; set; } = 0;
        public decimal ServiceTaxRate { get; set; } = 0;
        public decimal ServiceTaxAmount { get; set; } = 0;
        public decimal ExciseRate { get; set; } = 0;
        public decimal ExciseAmount { get; set; } = 0;
        public decimal InvoiceNetValue { get; set; } = 0;
        public long? MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual MaterialMaster fk_Material { get; set; }
        public long CnId { get; set; }
        [ForeignKey("CnId"), ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public virtual CNMaster fk_CN { get; set; }

        public decimal ActualQty { get; set; } = 1;
        public long? ActualQtyUnitId { get; set; }
        [ForeignKey("ActualQtyUnitId")]
        public virtual UnitMaster fk_ActualQtyUnit { get; set; }
        public decimal ActualWeight { get; set; } = 0;
        public long? ActualWeightUnitId { get; set; }
        [ForeignKey("ActualWeightUnitId")]
        public virtual UnitMaster fk_ActualWeightUnit { get; set; }
        public decimal ChargeWeight { get; set; } = 0;
        public long? ChargeWeightUnitId { get; set; }
        [ForeignKey("ChargeWeightUnitId")]
        public virtual UnitMaster fk_ChargeWeightUnit { get; set; }
       
        public decimal ChargeQty { get; set; } = 1;
        public long? ChargeQtyUnitId { get; set; }
        [ForeignKey("ChargeQtyUnitId")]
        public virtual UnitMaster fk_ChargeQtyUnit { get; set; }
        /// <summary>
        /// Gets or sets Total Package for Actual Qty.
        /// </summary>
        /// <value>Total Package.</value>
        public decimal TotalPackage { get; set; } = 0;
        public long? PkgUnitId { get; set; }
        [ForeignKey("PkgUnitId")]
        public virtual UnitMaster fk_PkgUnit { get; set; }
        public decimal Length { get; set; } = 0;
        public decimal Height { get; set; } = 0;
        public decimal Breadth { get; set; } = 0;
        public long? VolumeUnitId { get; set; }
        [ForeignKey("VolumeUnitId")]
        public virtual UnitMaster fk_VolumeUnit { get; set; }
        public decimal CFT { get; set; }
        public decimal Rate { get; set; }
        public decimal Freight { get; set; } = 0;
        [MaxLength(300)]
        public string Remark { get; set; }
        /// <summary>
        /// Gets or sets the ref1 identifier.
        /// </summary>
        /// <value>The ref1 identifier.</value>
        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_Ref1 { get; set; }
        [MaxLength(300)]
        public string Ref1 { get; set; }//GlovisLR
        [MaxLength(300)]
        public string Ref2 { get; set; }//TCILRNo
        [MaxLength(300)]
        public string Ref3 { get; set; }//ChassisNo
        [MaxLength(300)]
        public string Ref4 { get; set; }//Model

        [MaxLength(150)]
        public string EWayBillMM { get; set; }

        public DateTime? eWayBillValidity { get; set; }

        public decimal AddI { get; set; }
        public decimal AddII { get; set; }
        public decimal AddIII { get; set; }
        public decimal AddIV { get; set; }
        public decimal AddV { get; set; }
        public decimal AddVI { get; set; }
        public decimal AddVII { get; set; }
        public decimal AddVIII { get; set; }
        public decimal LessI { get; set; }
        public decimal LessII { get; set; }
        public decimal LessIII { get; set; }
        public decimal LessIV { get; set; }
        public decimal LessV { get; set; }
        public decimal LessVI { get; set; }
        public decimal LessVII { get; set; }
        public decimal LessVIII { get; set; }
        public decimal NetFreight { get; set; }
        public decimal InvoiceRate { get; set; }
    }
}