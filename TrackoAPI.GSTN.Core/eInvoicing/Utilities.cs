using System;

namespace TrackoAPI.GSTN.Core.eInvoicing
{
    public class Utilities
    {
        public static string ComputeIRN(string gstNumber, DOCTYPE docType, string invoiceNo,DateTime invoiceDate)
        {
            return $"{gstNumber}{GetFYByDate(invoiceDate)}{docType}{invoiceNo}";
        }
        public static string GetFYByDate(
            DateTime invoiceDate)
        {
            var fy_startdate = new DateTime(invoiceDate.Year, 4, 1);
            if(fy_startdate>= invoiceDate)
            {
                return $"{invoiceDate.Year-1}-{invoiceDate:yy}";
            }
            return $"{invoiceDate:yyyy}-{invoiceDate.AddYears(1):yy}";
        }
    }
    public enum DOCTYPE
    {
        /// <summary>
        /// Invoice
        /// </summary>
        INV,
        /// <summary>
        /// CreditNote
        /// </summary>
        CRN,
        /// <summary>
        /// DebitNote
        /// </summary>
        DBN
    }
}
