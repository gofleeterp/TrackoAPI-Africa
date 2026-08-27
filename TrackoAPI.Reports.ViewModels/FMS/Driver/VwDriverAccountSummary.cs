namespace TrackoAPI.Reports.ViewModels.FMS.Driver
{
    public class VwDriverAccountSummary
    {
        public long DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverCode { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? TripAdvAmount { get; set; }
        public decimal? TripExpAmount { get; set; }
        public decimal? UnSettledAdvAmount { get; set; }
    }
}
