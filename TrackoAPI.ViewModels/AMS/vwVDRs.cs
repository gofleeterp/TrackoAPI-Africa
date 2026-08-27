using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.AMS
{
    public class FakeVDRs
    {
        public long? AVDRId { get; set; }/*Original VDRId*/
        /// <summary>
        /// Balance Adjusted by User in Advance against new Ref
        /// </summary>
        public decimal Adjusted { get; set; }
        public decimal CurRate { get; set; }
        public long? CurTypeId { get; set; }
        /// <summary>
        /// New Reference Balance Amount in Landing Currency
        /// </summary>
        public decimal BalanceInLandingValueId { get; set; }
        /// <summary>
        /// Adjustment Balance without manual Intervention
        /// </summary>
        public decimal AdjustedId { get; set; }
        public decimal Balance { get; set; }
        /// <summary>
        /// New Reference Balance Amount in Document Currency
        /// </summary>
        public decimal BalanceInDocmentValueId { get; set; }
        public bool IsDeleteId { get; set; } = false;
        public long? TransactionId { get; set; }
        public long? AccountId { get; set; }
        public int VoucherTypeId { get; set; }
        public int FyId { get; set; } = 0;
    }
}
