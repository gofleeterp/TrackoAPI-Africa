using System;
using System.Data.Entity.Core.Objects.DataClasses;

namespace TrackoAPI.ViewModels.BMS
{
    [EdmComplexType]
    public class vwCNStockSearch
    {
        public long Id { get; set; }
        public string CNNo { get; set; }
        public long CNId { get; set; }
        public DateTime StockDate { get; set; }
        public long StockOfficeId { get; set; }
        public decimal StockQty{ get; set; }
    }
    [EdmComplexType]
    public class vwCNStockMMSearch
    {
        public long Id { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public long MaterialId { get; set; }
        public decimal StockQty { get; set; }
        public string CNNo { get; set; }
        public long CNId { get; set; }
        public DateTime StockDate { get; set; }
        public long StockOfficeId { get; set; }
        
    }
    [EdmComplexType]
    public class vwCNStockMM
    {
        public long Id { get; set; }
        public string CNNo { get; set; }
        public long CNId { get; set; }
        public DateTime StockInDate { get; set; }
        public long StockOfficeId { get; set; }
        public long? MaterialId { get; set; }
        public string Material { get; set; }
        public string MaterialGroup { get; set; }
        public string MaterialCode { get; set; }
        public string Vendor { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal BookedQty { get; set; }
    }

    public class vwBatch
    {
        public string BatchId { get; set; }
        public int BatchSize { get; set; }
    }
}
