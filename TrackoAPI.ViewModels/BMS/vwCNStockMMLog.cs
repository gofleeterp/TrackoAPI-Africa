using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.BMS
{
    [EdmComplexType]
    public class vwCNStockMMLog
    {
        public long Id { get; set; }
        public string PartName { get; set; }
        public long StockLogId { get; set; }

        public long MaterialId { get; set; }

        public long CNMMId { get; set; }
        public DateTime? LogDate { get; set; }
        public long CNId { get; set; }
        public long OfficeId { get; set; }
        public decimal InQty { get; set; } = 0;
        public decimal ShortageQty { get; set; } = 0;
        public decimal DamagedQty { get; set; } = 0;
        public decimal ExessQty { get; set; } = 0;
        public decimal OutQty { get; set; } = 0;
        public long LogTypeId { get; set; }

        public long? ChallanCNId { get; set; }

        public long? RefStockId { get; set; }
        public long? TriplogId { get; set; }

        [MaxLength(200)]
        public string Ref1 { get; set; }

        [MaxLength(200)]
        public string Ref2 { get; set; }
        public DateTime? Date2 { get; set; }
    }

    public class vw_CNStockLog
    {
        //[Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long CNId { get; set; }
        public DateTime LogDate { get; set; }
        public long OfficeId { get; set; }
        public long? ChallanCNId { get; set; }
        public long? TriplogId { get; set; }
        public long? RefStockId { get; set; }
        public decimal InQty { get; set; }
        public decimal BalanceQty { get; set; }
    }
    public class vw_CNStockMMLog
    {
        //[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long StockLogId { get; set; }
        public long CNMMId { get; set; }
        public long MaterialId { get; set; }
        public long CNId { get; set; }
        public DateTime LogDate { get; set; }
        public long OfficeId { get; set; }
        public long? ChallanCNId { get; set; }
        public long? TriplogId { get; set; }
        public long? RefStockId { get; set; }
        public decimal InQty { get; set; } = 0;
        public decimal BalanceQty { get; set; } = 0;
    }
    public class VW_DispatchAcknowledgment
    {
        public long Id { get; set; }
        public long? LogType { get; set; }
        public string AcknowledgmentNo { get; set; }
        public string VendorCode { get; set; }
        public string PartNo { get; set; }
        public string InvoiceNo { get; set; }
        public decimal Qty { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string ChallanNo { get; set; }
        public decimal OutQtyId { get; set; }
        public long? DispatchId { get; set; }
    }
}
