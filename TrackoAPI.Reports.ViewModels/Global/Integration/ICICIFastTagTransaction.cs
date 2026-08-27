using System;
using System.Collections.Generic;

namespace TrackoAPI.Reports.ViewModels.Global.Integration
{
    /// <summary>
    /// 
    /// </summary>
    public class ICICIFastTagTransactionRequest : BaseICICIRequest
    {
        /// <summary>
        /// Specifies Vehicle number - Mandatory field
        /// </summary>
        public string VehicleNumber { get; set; }
        /// <summary>
        /// Start date of the duration for requesting the transactions dated number - Mandatory and should be send in ISO8601 format.
        ///Transaction details can be retrieved for a maximum of past 40 days.Start Transaction Date should be less than or equal to that of today’s date.
        /// </summary>
        public DateTimeOffset StartTransactionDate { get; set; }
        /// <summary>
        /// End date of the duration for requesting the transaction dated to, Date should be send in ISO8601format.
        /// End date should not be future date.
        /// </summary>
        public DateTimeOffset EndTransactionDate { get; set; } = DateTimeOffset.UtcNow.ToOffset(new TimeSpan(5, 30, 0));
        public int PageNo { get; set; }
        public bool IsPagingRequired { get; set; } = false;
    }
    public class ICICIFastTagTransactionResponse : BaseICICIRequest
    {
        /// <summary>
        /// Specifies Vehicle number - Mandatory field
        /// </summary>
        public string VehicleNumber { get; set; }
        /// <summary>
        /// Contains collection of Transaction details that needs to be returned for the requested vehicle number and duration in the request.
        /// </summary>
        public List<ICICIFastTagTransactionDetail> TransactionDetails { get; set; }

        public int TotalPages { get; set; }
        public int CurrentPageNumber { get; set; }
        public int NoofTxnsForEachPage { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class ICICIFastTagTransactionDetail
    {
        public long VehicleId { get; set; }
        /// <summary>
        /// Transaction ID of the transaction.
        /// </summary>
        public long TransactionId { get; set; }
        /// <summary>
        /// Lane Code of the transaction.
        /// </summary>
        public string LaneCode { get; set; }
        /// <summary>
        /// Plaza Code of the transaction.
        /// </summary>
        public string PlazaCode { get; set; }
        /// <summary>
        /// Transaction Amount.
        /// </summary>
        public decimal TransactionAmount { get; set; }
        /// <summary>
        /// Date time of transaction. Returns in ISO 8601 date format
        /// </summary>
        public DateTimeOffset TransactionDateTime { get; set; }
        /// <summary>
        /// Processed date time of the transaction. Returns in ISO 8601 date format
        /// </summary>
        public DateTimeOffset ProcessingDateTime { get; set; }
        /// <summary>
        /// Reference number of the transaction.
        /// </summary>
        public string TransactionReferenceNumber { get; set; }
        /// <summary>
        /// Plaza Name of the transaction.
        /// </summary>
        public string PlazaName { get; set; }
        /// <summary>
        /// Transaction of the transaction.
        /// </summary>
        public string TransactionStatus { get; set; }

        public string VehicleNumber { get; set; }
    }
}
