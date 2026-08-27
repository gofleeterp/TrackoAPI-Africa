using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System.Data.Entity.Core.Objects.DataClasses;

namespace TrackoAPI.ViewModels.BMS
{
    [EdmComplexType]
    public class vwEWayBill
    {
        public long? ConsignorId { get; set; }
        public string EWayBillNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? HireVehicleId { get; set; }
        public string InvoiceNo { get; set; }
        public long? CNId { get; set; }
        public long? TripLogId { get; set; }
    }
    [EdmComplexType]
    public class vwCNMultiMaterial
    {
        public long Id { get; set; }
        [MaxLength(200)]
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public decimal InvoiceValue { get; set; } = 0;
        public decimal ServiceTaxRate { get; set; } = 0;
        public decimal ServiceTaxAmount { get; set; } = 0;
        public decimal ExciseRate { get; set; } = 0;
        public decimal ExciseAmount { get; set; } = 0;
        public decimal InvoiceNetValue { get; set; } = 0;
        public long? MaterialId { get; set; }
        public decimal ActualQty { get; set; }
        public long? ActualQtyUnitId { get; set; }
        public decimal ActualWeight { get; set; } = 0;
        public long? ActualWeightUnitId { get; set; }
        public decimal ChargeWeight { get; set; } = 0;
        public long? ChargeWeightUnitId { get; set; }

        public decimal ChargeQty { get; set; }
        public long? ChargeQtyUnitId { get; set; }
        /// <summary>
        /// Gets or sets Total Package for Actual Qty.
        /// </summary>
        /// <value>Total Package.</value>
        public decimal TotalPackage { get; set; } = 0;
        public long? PkgUnitId { get; set; }
        public decimal Length { get; set; } = 0;
        public decimal Height { get; set; } = 0;
        public decimal Breadth { get; set; } = 0;
        public long? VolumeUnitId { get; set; }
        public decimal CFT { get; set; }
        public decimal Rate { get; set; }
        public decimal Freight { get; set; } = 0;
        public string Remark { get; set; }
        /// <summary>
        /// Gets or sets the ref1 identifier.
        /// </summary>
        /// <value>The ref1 identifier.</value>
        public long? Ref1Id { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public decimal InvoiceRate { get; set; } = 0;
        [MaxLength(200)]
        public string EWayBillMM { get; set; }
        public DateTime? eWayBillValidity { get; set; }
        public bool IsDeleted { get; set; }
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
    }
}
