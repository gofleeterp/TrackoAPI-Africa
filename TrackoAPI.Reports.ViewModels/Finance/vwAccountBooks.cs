using System;

namespace TrackoAPI.Reports.ViewModels.Finance
{
    public class vwAccountLedger
    {
        public long VoucherId { get; set; }
        public long AccountId { get; set; }
        public long VdId { get; set; }
        public long? VOfficeId { get; set; }
        public long? VdOfficeId { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string Office { get; set; }
        public string VoucherNo { get; set; }
        public string ChequeNo { get; set; }
        public string VoucherType { get; set; }
        public string Particulars { get; set; }
        public string RefNos { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Narration { get; set; }

    }
    public class vwDayBook
    {
        public long VoucherId { get; set; }
        public long AccountId { get; set; }
        public long VdId { get; set; }
        public long? VOfficeId { get; set; }
        public long? VdOfficeId { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string Office { get; set; }
        public string VoucherNo { get; set; }
        public string ChequeNo { get; set; }
        public string VoucherType { get; set; }
        public string Particulars { get; set; }
        public string RefNos { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Narration { get; set; }

    }
}
